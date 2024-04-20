using System.IO;
using System.Windows;

namespace SearchWordApp;
// C:\Users\snizh\OneDrive\Робочий стіл\exam
public partial class MainWindow : Window
{
    private int _filesChecked;
    private int _totalFiles;
    private object _locker = new object();

    public MainWindow()
    {
        InitializeComponent();
        pb_process.Value = 0;
    }

    private async void SearchButtonOnClick(object sender, RoutedEventArgs e)
    {
        resetData();
        tb_data.Text = "Checking files..." + Environment.NewLine;
        string word = tb_word.Text;
        string path = tb_path.Text;


        List<string> files = new List<string>();
        if (path == "" || word == "")
        {
            tb_data.Text = "Please enter the path and word";
            return;
        }
        else {
            await Task.Run(() =>
            {

                GetFolders(path, files);
                Parallel.ForEach(files, (file) =>
                {
                    ReadFile(file, word).Wait();
                    _totalFiles = files.Count;
                }); //cинхронно чекає wait 
            });
        }
    }

    private async Task ReadFile(string path, string word)
    {

        
        int countWord = 0;
        using (FileStream fs = new FileStream(path, FileMode.Open))
        {
            using (StreamReader sr = new StreamReader(fs))
            {
                while (!sr.EndOfStream)
                {
                    string? text = await sr.ReadLineAsync();
                    if (text != null && text.Contains(word))
                    {
                        countWord++;
                    }
                }
            }
        }
        lock (_locker)
        {
            _filesChecked++;
            if (_totalFiles != 0)
            {
                Dispatcher.Invoke(() => pb_process.Value = (double)_filesChecked / _totalFiles * 100);
            }
            Dispatcher.Invoke(() => textBlock.Text = $"{_filesChecked}/{_totalFiles} files");

        }

        Dispatcher.Invoke(() => tb_data.Text += $"{System.IO.Path.GetFileName(path)} | {System.IO.Path.GetFullPath(path)} | {countWord}{Environment.NewLine}");
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

    private void resetData()
    {
        _filesChecked = 0;
        pb_process.Value = 0;
        tb_data.Text = "";
        textBlock.Text = "";
    }
}