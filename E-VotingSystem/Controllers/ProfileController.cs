using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;

public class ProfileController : Controller
{
    private const string G_Executive = "Executive";
    private const string G_Local = "Local";

    [HttpGet]
    public ActionResult Index()
    {
        ModMember? l_ModLoggedInMember = HttpContext.Session.Get<ModMember>("LoggedInMember");
        if (l_ModLoggedInMember == null)
        {
            return RedirectToAction("Index", "Account");
        }

        return View(l_ModLoggedInMember);
    }

    [HttpGet]
    public ActionResult Local()
    {
        try
        {
            ModMember? l_ModLoggedInMember = HttpContext.Session.Get<ModMember>("LoggedInMember");
            if (l_ModLoggedInMember == null)
            {
                return RedirectToAction("Index", "Account");
            }

            CmConnectionHelper l_CmConnectionHelper = new CmConnectionHelper();
            string l_ConnectionString = l_CmConnectionHelper.Fnc_GetConnectionString();

            using (SqlConnection l_SqlConnection_Vote = new SqlConnection(l_ConnectionString))
            {
                l_SqlConnection_Vote.Open();
                SqlCommand l_SqlCommand_Vote = new SqlCommand();
                l_SqlCommand_Vote.Connection = l_SqlConnection_Vote;
                l_SqlCommand_Vote.Parameters.AddWithValue("@mMemberDID", l_ModLoggedInMember.PKGUID);
                l_SqlCommand_Vote.CommandText = $" SELECT COUNT(*) AS VoteCount FROM TBU_Vote WHERE MemberDID = @mMemberDID  AND NominatedFor = '{G_Local}' ; ";

                object l_Result = l_SqlCommand_Vote.ExecuteScalar();
                if (l_Result == DBNull.Value)
                {
                    TempData["ErrorMessage"] = "Error in fetching casted vote count from database";
                    return View("Index");
                }

                int l_VoteCount = (int)l_Result;

                if (l_VoteCount >= l_ModLoggedInMember.LcCouncilSeats)
                {
                    return View("ErrorLocalVoteLimit", l_ModLoggedInMember);
                }
            }

            List<ModCandidate> l_ListModCandidate_Local = new List<ModCandidate>();

            using (SqlConnection l_SqlConnection = new SqlConnection(l_ConnectionString))
            {
                l_SqlConnection.Open();
                SqlCommand l_SqlCommand = new SqlCommand();
                l_SqlCommand.Connection = l_SqlConnection;
                l_SqlCommand.Parameters.AddWithValue("@mRegion", l_ModLoggedInMember.Region);
                l_SqlCommand.CommandText = $" SELECT * FROM TBU_Candidate WHERE Region = @mRegion AND NominatedFor = '{G_Local}' ";


                SqlDataReader l_SqlDataReader = l_SqlCommand.ExecuteReader();
                while (l_SqlDataReader.Read() == true)
                {
                    ModCandidate l_ModCandidate = new ModCandidate();
                    l_ModCandidate.PKGUID = (Guid)l_SqlDataReader["PKGUID"];
                    l_ModCandidate.CandidateID = (string)l_SqlDataReader["CandidateID"];
                    l_ModCandidate.CandidateName = (string)l_SqlDataReader["CandidateName"];
                    l_ModCandidate.NominatedFor = (string)l_SqlDataReader["NominatedFor"];
                    l_ModCandidate.Region = (string)l_SqlDataReader["Region"];
                    l_ModCandidate.Email = (string)l_SqlDataReader["Email"];
                    l_ModCandidate.ContactNo = (string)l_SqlDataReader["ContactNo"];
                    l_ModCandidate.Mobile = (string)l_SqlDataReader["Mobile"];
                    l_ModCandidate.Address = (string)l_SqlDataReader["Address"];
                    l_ModCandidate.Image = (string)l_SqlDataReader["Image"];
                    l_ModCandidate.IsVote = false;

                    l_ListModCandidate_Local.Add(l_ModCandidate);
                }
            }

            TempData["LoggedInMember"] = l_ModLoggedInMember;
            return View("Local", l_ListModCandidate_Local);
        }
        catch (Exception ex)
        {
            new CmConnectionHelper().Vd_WriteToFile(ex.Message);
            TempData["ErrorMessage"] = ex.Message;
            return View("Index");
        }
    }

    [HttpGet]
    public ActionResult Executive()
    {
        try
        {
            ModMember? l_ModLoggedInMember = HttpContext.Session.Get<ModMember>("LoggedInMember");
            if (l_ModLoggedInMember == null)
            {
                return RedirectToAction("Index", "Account");
            }

            CmConnectionHelper l_CmConnectionHelper = new CmConnectionHelper();
            string l_ConnectionString = l_CmConnectionHelper.Fnc_GetConnectionString();

            using (SqlConnection l_SqlConnection_Vote = new SqlConnection(l_ConnectionString))
            {
                l_SqlConnection_Vote.Open();
                SqlCommand l_SqlCommand_Vote = new SqlCommand();
                l_SqlCommand_Vote.Connection = l_SqlConnection_Vote;
                l_SqlCommand_Vote.Parameters.AddWithValue("@mMemberDID", l_ModLoggedInMember.PKGUID);
                l_SqlCommand_Vote.CommandText = $" SELECT COUNT(*) AS VoteCount FROM TBU_Vote WHERE MemberDID = @mMemberDID  AND NominatedFor = '{G_Executive}' ; ";

                object l_Result = l_SqlCommand_Vote.ExecuteScalar();
                if (l_Result == DBNull.Value)
                {
                    TempData["ErrorMessage"] = "Error in fetching casted vote count from database";
                    return View("Index");
                }

                int l_VoteCount = (int)l_Result;

                if (l_VoteCount >= l_ModLoggedInMember.ExCouncilSeats)
                {
                    return View("ErrorExecutiveVoteLimit", l_ModLoggedInMember);
                }
            }

            List<ModCandidate> l_ListModCandidate_Executive = new List<ModCandidate>();

            using (SqlConnection l_SqlConnection = new SqlConnection(l_ConnectionString))
            {
                l_SqlConnection.Open();
                SqlCommand l_SqlCommand = new SqlCommand();
                l_SqlCommand.Connection = l_SqlConnection;
                l_SqlCommand.Parameters.AddWithValue("@mRegion", l_ModLoggedInMember.Region);
                l_SqlCommand.CommandText = $" SELECT * FROM TBU_Candidate WHERE Region = @mRegion AND NominatedFor = '{G_Executive}' ";


                SqlDataReader l_SqlDataReader = l_SqlCommand.ExecuteReader();
                while (l_SqlDataReader.Read() == true)
                {
                    ModCandidate l_ModCandidate = new ModCandidate();
                    l_ModCandidate.PKGUID = (Guid)l_SqlDataReader["PKGUID"];
                    l_ModCandidate.CandidateID = (string)l_SqlDataReader["CandidateID"];
                    l_ModCandidate.CandidateName = (string)l_SqlDataReader["CandidateName"];
                    l_ModCandidate.NominatedFor = (string)l_SqlDataReader["NominatedFor"];
                    l_ModCandidate.Region = (string)l_SqlDataReader["Region"];
                    l_ModCandidate.Email = (string)l_SqlDataReader["Email"];
                    l_ModCandidate.ContactNo = (string)l_SqlDataReader["ContactNo"];
                    l_ModCandidate.Mobile = (string)l_SqlDataReader["Mobile"];
                    l_ModCandidate.Address = (string)l_SqlDataReader["Address"];
                    l_ModCandidate.Image = (string)l_SqlDataReader["Image"];
                    l_ModCandidate.IsVote = false;

                    l_ListModCandidate_Executive.Add(l_ModCandidate);
                }
            }

            TempData["LoggedInMember"] = l_ModLoggedInMember;
            return View("Executive", l_ListModCandidate_Executive);
        }
        catch (Exception ex)
        {
            new CmConnectionHelper().Vd_WriteToFile(ex.Message);
            TempData["ErrorMessage"] = ex.Message;
            return View("Index");
        }
    }

    [HttpPost]
    public ActionResult CastVoteLocal(List<ModCandidate> l_ListModCandidate_Local)
    {
        try
        {
            ModMember? l_ModLoggedInMember = HttpContext.Session.Get<ModMember>("LoggedInMember");
            if (l_ModLoggedInMember == null)
            {
                return View(new List<ModMember>());
            }

            int l_TotalVoteCount = l_ListModCandidate_Local.Where(x => x.IsVote == true).Count();

            if (l_TotalVoteCount < l_ModLoggedInMember.LcCouncilSeats)
            {
                TempData["ErrorMessage"] = $"You have to select at least {l_ModLoggedInMember.LcCouncilSeats} local candidates";
                return View("Local", l_ListModCandidate_Local);
            }

            if (l_TotalVoteCount > l_ModLoggedInMember.LcCouncilSeats)
            {
                TempData["ErrorMessage"] = $"You can not select more than {l_ModLoggedInMember.LcCouncilSeats} local candidates";
                return View("Local", l_ListModCandidate_Local);
            }

            IEnumerableVote l_IEnumerableVote = new IEnumerableVote();
            foreach (ModCandidate l_ModCandidate in l_ListModCandidate_Local.Where(x => x.IsVote == true))
            {
                ModVote l_ModVote = new ModVote();
                l_ModVote.PKGUID = Guid.NewGuid();
                l_ModVote.MemberDID = l_ModLoggedInMember.PKGUID;
                l_ModVote.CandidateDID = l_ModCandidate.PKGUID;
                l_ModVote.NominatedFor = G_Local;
                l_ModVote.VoteDate = DateTime.Now;

                l_IEnumerableVote.Add(l_ModVote);
            }

            CmConnectionHelper l_CmConnectionHelper = new CmConnectionHelper();
            string l_ConnectionString = l_CmConnectionHelper.Fnc_GetConnectionString();
            using (SqlConnection l_SqlConnection = new SqlConnection(l_ConnectionString))
            {
                l_SqlConnection.Open();

                SqlCommand l_SqlCommand = new SqlCommand("Pr_TBU_Vote_CRUDm", l_SqlConnection);
                l_SqlCommand.CommandType = CommandType.StoredProcedure;
                l_SqlCommand.Parameters.Add("@mTP_Vote_CRUDm", SqlDbType.Structured);
                l_SqlCommand.Parameters["@mTP_Vote_CRUDm"].Value = l_IEnumerableVote;
                l_SqlCommand.Parameters["@mTP_Vote_CRUDm"].Direction = ParameterDirection.Input;

                l_SqlCommand.ExecuteNonQuery();
            }

            using (SqlConnection l_SqlConnection_Vote = new SqlConnection(l_ConnectionString))
            {
                l_SqlConnection_Vote.Open();
                SqlCommand l_SqlCommand_Vote = new SqlCommand();
                l_SqlCommand_Vote.Connection = l_SqlConnection_Vote;
                l_SqlCommand_Vote.Parameters.AddWithValue("@mMemberDID", l_ModLoggedInMember.PKGUID);
                l_SqlCommand_Vote.CommandText = $" SELECT COUNT(*) AS VoteCount FROM TBU_Vote WHERE MemberDID = @mMemberDID  AND NominatedFor = '{G_Executive}' ; ";

                object l_Result = l_SqlCommand_Vote.ExecuteScalar();
                if (l_Result == DBNull.Value)
                {
                    TempData["ErrorMessage"] = "Error in fetching casted vote count from database";
                    return View("Index");
                }

                int l_VoteCount = (int)l_Result;

                if (l_VoteCount >= l_ModLoggedInMember.LcCouncilSeats)
                {
                    TempData["IsVotesCastedExecutive"] = true;
                }
                else
                {
                    TempData["IsVotesCastedExecutive"] = false;
                }
            }

            TempData["IsVotesCastedLocal"] = true;     
            return View("VotesCasted");
        }
        catch (Exception ex)
        {
            new CmConnectionHelper().Vd_WriteToFile(ex.Message);
            TempData["ErrorMessage"] = ex.Message;
            return View("Index");
        }
    }


    [HttpPost]
    public ActionResult CastVoteExecutive(List<ModCandidate> l_ListModCandidate_Executive)
    {
        try
        {
            ModMember? l_ModLoggedInMember = HttpContext.Session.Get<ModMember>("LoggedInMember");
            if (l_ModLoggedInMember == null)
            {
                return View(new List<ModMember>());
            }

            int l_TotalVoteCount = l_ListModCandidate_Executive.Where(x => x.IsVote == true).Count();

            if (l_TotalVoteCount < l_ModLoggedInMember.ExCouncilSeats)
            {
                TempData["ErrorMessage"] = $"You have to select at least {l_ModLoggedInMember.ExCouncilSeats} executive candidates";
                return View("Executive", l_ListModCandidate_Executive);
            }

            if (l_TotalVoteCount > l_ModLoggedInMember.ExCouncilSeats)
            {
                TempData["ErrorMessage"] = $"You can not select more than {l_ModLoggedInMember.ExCouncilSeats} executive candidates";
                return View("Executive", l_ListModCandidate_Executive);
            }

            IEnumerableVote l_IEnumerableVote = new IEnumerableVote();
            foreach (ModCandidate l_ModCandidate in l_ListModCandidate_Executive.Where(x => x.IsVote == true))
            {
                ModVote l_ModVote = new ModVote();
                l_ModVote.PKGUID = Guid.NewGuid();
                l_ModVote.MemberDID = l_ModLoggedInMember.PKGUID;
                l_ModVote.CandidateDID = l_ModCandidate.PKGUID;
                l_ModVote.NominatedFor = G_Executive;
                l_ModVote.VoteDate = DateTime.Now;

                l_IEnumerableVote.Add(l_ModVote);
            }

            CmConnectionHelper l_CmConnectionHelper = new CmConnectionHelper();
            string l_ConnectionString = l_CmConnectionHelper.Fnc_GetConnectionString();
            using (SqlConnection l_SqlConnection = new SqlConnection(l_ConnectionString))
            {
                l_SqlConnection.Open();

                SqlCommand l_SqlCommand = new SqlCommand("Pr_TBU_Vote_CRUDm", l_SqlConnection);
                l_SqlCommand.CommandType = CommandType.StoredProcedure;
                l_SqlCommand.Parameters.Add("@mTP_Vote_CRUDm", SqlDbType.Structured);
                l_SqlCommand.Parameters["@mTP_Vote_CRUDm"].Value = l_IEnumerableVote;
                l_SqlCommand.Parameters["@mTP_Vote_CRUDm"].Direction = ParameterDirection.Input;

                l_SqlCommand.ExecuteNonQuery();
            }

            using (SqlConnection l_SqlConnection_Vote = new SqlConnection(l_ConnectionString))
            {
                l_SqlConnection_Vote.Open();
                SqlCommand l_SqlCommand_Vote = new SqlCommand();
                l_SqlCommand_Vote.Connection = l_SqlConnection_Vote;
                l_SqlCommand_Vote.Parameters.AddWithValue("@mMemberDID", l_ModLoggedInMember.PKGUID);
                l_SqlCommand_Vote.CommandText = $" SELECT COUNT(*) AS VoteCount FROM TBU_Vote WHERE MemberDID = @mMemberDID  AND NominatedFor = '{G_Local}' ; ";

                object l_Result = l_SqlCommand_Vote.ExecuteScalar();
                if (l_Result == DBNull.Value)
                {
                    TempData["ErrorMessage"] = "Error in fetching casted vote count from database";
                    return View("Index");
                }

                int l_VoteCount = (int)l_Result;

                if (l_VoteCount >= l_ModLoggedInMember.LcCouncilSeats)
                {
                    TempData["IsVotesCastedLocal"] = true;
                }
                else
                {
                    TempData["IsVotesCastedLocal"] = false;
                }
            }

            TempData["IsVotesCastedExecutive"] = true;         
            return View("VotesCasted");
        }
        catch (Exception ex)
        {
            new CmConnectionHelper().Vd_WriteToFile(ex.Message);
            TempData["ErrorMessage"] = ex.Message;
            return View("Index");
        }
    }
}