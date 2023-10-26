public class CmConnectionHelper
{
    public string FncGetConnectionString()
    {
        //return "Data Source=MUHAMMAD-UMAIR\\AISONESQL;Initial Catalog=EVoting;Integrated Security=True";
        return "Data Source = (local)\\AisoneSQL; User ID = sa; Pwd = Smc786<>; Initial Catalog = EVoting; Connection Timeout = 3000; Integrated Security = False;";
    }

    public void WriteToFile(string l_Text)
    {
        try
        {
            string l_Path = Path.Combine("C:\\ProgramData", "EVoting_Exception.txt");
            File.WriteAllText(l_Path,l_Text);          
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error writing to the file: " + ex.Message);
        }
    }
}