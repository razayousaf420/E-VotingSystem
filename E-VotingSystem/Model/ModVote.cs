public class ModVote
{
    public Guid PKGUID { get; set; }
    public Guid MemberDID { get; set; }
    public Guid CandidateDID { get; set; }
    public string? NominatedFor { get; set; }
    public DateTime VoteDate { get; set; }
}