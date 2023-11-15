public class ModCanidateResultDataSet
{
    public List<ModRptCandidateResult> ListModRptCandidateResult_Local { get; set; } = new List<ModRptCandidateResult>  ();
    public List<ModRptCandidateResult> ListModRptCandidateResult_Executive { get; set; } = new List<ModRptCandidateResult>();   
}

public class ModRptCandidateResult
{
    public Int64? SrNo { get; set; }
    public Guid PKGUID { get; set; }
    public string? CandidateID { get; set; }
    public string? CandidateName { get; set; }
    public string? Region { get; set; }
    public int VoteCount { get; set; }
    public string? Image { get; set; }
}