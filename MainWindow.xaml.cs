using System.IO;
using System.Windows;

namespace SearchWordApp;

public partial class MainWindow : Window
{
    private int filesChecked;
    private int totalFiles;
    private object locker = new object();

    public MainWindow()
    {
        InitializeComponent();
        pb_process.Value = 0;
    }

    private async void Button_Click(object sender, RoutedEventArgs e)
    {
        tb_data.Text = "Checking files..." + Environment.NewLine;
        string word = tb_word.Text;
        string path = tb_path.Text;


        List<string> files = new List<string>();
        await Task.Run(() =>
        {
            GetFolders(path, files);
            Parallel.ForEach(files, (file) =>
            {
                ReadFile(file, word).Wait();
                totalFiles = files.Count;
            }); //cинхронно чекає wait 
        });
    }

    private async Task ReadFile(string path, string word)
    {

        int count = 0;
        using (FileStream fs = new FileStream(path, FileMode.Open))
        {
            using (StreamReader sr = new StreamReader(fs))
            {
                while (!sr.EndOfStream)
                {
                    string? text = await sr.ReadLineAsync();
                    if (text != null && text.Contains(word))
                    {
                        count++;
                    }
                }
            }
        }
        lock (locker)
        {
            filesChecked++;
            if (totalFiles != 0)
            {
                Dispatcher.Invoke(() => pb_process.Value = (double)filesChecked / totalFiles * 100);
            }
            Dispatcher.Invoke(() => textBlock.Text = $"{filesChecked}/{totalFiles} files");

        }

        Dispatcher.Invoke(() => tb_data.Text += $"{System.IO.Path.GetFileName(path)} | {System.IO.Path.GetFullPath(path)} | {count}{Environment.NewLine}");
        await Task.Delay(1000);

    }

    private static void GetFolders(string path, List<string> files)
    {

        foreach (var file in Directory.EnumerateFiles(path))
        {
            files.Add(file);
        }
        foreach (var folder in Directory.EnumerateDirectories(path))
        {
            GetFolders(folder, files);
        }
    }
}