using CommandLine;
using RFilesCLI;

Parser.Default.ParseArguments<
    ListCommand, 
    ListHashCommand, 
    QueryCommand, 
    HeadCommand, 
    DeleteCommand, 
    UploadCommand, 
    DownloadCommand>(args).MapResult(
        (ListCommand c) => c.Run(),
        (ListHashCommand c) => c.Run(),
        (QueryCommand c) => c.Run(),
        (HeadCommand c) => c.Run(),
        (DeleteCommand c) => c.Run(),
        (UploadCommand c) => c.Run(),
        (DownloadCommand c) => c.Run(),
        errors => 
        {
            foreach (var err in errors)
            {
                Console.WriteLine(err);
            }
            return 1;
        }
    );