public class CmConnectionHelper
{
    public string Fnc_GetConnectionString()
    {      
        return "Data Source = (local)\\AisoneSQL; User ID = sa; Pwd = Smc786<>; Initial Catalog = EVoting; Connection Timeout = 3000; Integrated Security = False;";
    }

    public void Vd_WriteToFile(string l_Text)
    {
        string l_Path = Path.Combine("C:\\ProgramData", "EVoting_Exception.txt");
        File.WriteAllText(l_Path, l_Text);
    }
}