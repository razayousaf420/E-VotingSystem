using System.Data;
using Microsoft.SqlServer.Server;

public class IEnumerableVote : List<ModVote>, IEnumerable<SqlDataRecord>
{
    IEnumerator<SqlDataRecord> IEnumerable<SqlDataRecord>.GetEnumerator() => Pb_Fnc_GenerateCollection();
    public IEnumerator<SqlDataRecord> Pb_Fnc_GenerateCollection()
    {
        SqlDataRecord l_SqlDataRecord = new SqlDataRecord(
            new SqlMetaData("PKGUID", SqlDbType.UniqueIdentifier),
            new SqlMetaData("MemberDID", SqlDbType.UniqueIdentifier),
            new SqlMetaData("CandidateDID", SqlDbType.UniqueIdentifier),
            new SqlMetaData("NominatedFor", SqlDbType.NVarChar, 255),
            new SqlMetaData("VoteDate", SqlDbType.DateTime));

        foreach (ModVote l_ModVote in this)
        {
            l_SqlDataRecord.SetGuid(0, l_ModVote.PKGUID);
            l_SqlDataRecord.SetGuid(1, l_ModVote.MemberDID);
            l_SqlDataRecord.SetGuid(2, l_ModVote.CandidateDID);
            l_SqlDataRecord.SetString(3, l_ModVote.NominatedFor);
            l_SqlDataRecord.SetDateTime(4, l_ModVote.VoteDate);
            yield return l_SqlDataRecord;
        }
    }
}