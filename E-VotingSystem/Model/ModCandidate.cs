public class ModCandidate
{
    public Guid PKGUID { get; set; }
    public string? CandidateID { get; set; }
    public string? CandidateName { get; set; }
    public string? NominatedFor { get; set; }
    public string? Region { get; set; }
    public string? Email { get; set; }
    public string? ContactNo { get; set; }
    public string? Mobile { get; set; }
    public string? Address { get; set; }
    public string? Image { get; set; }

    //UI

    public bool IsVote { get; set; }
}