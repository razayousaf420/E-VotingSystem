﻿using System.Data;
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
    public ActionResult UpdatePassword()
    {
        return View();
    }

    [HttpPost]
    public ActionResult Password(string l_NewPassword, string l_ConfirmPassword = "")
    {
        if (l_NewPassword != l_ConfirmPassword)
        {
            TempData["ErrorMessage"] = $"The new password and confirmed password field should be the same.";
            return View("UpdatePassword");
        }

        ModMember? l_ModLoggedInMember = HttpContext.Session.Get<ModMember>("LoggedInMember");
        if (l_ModLoggedInMember == null)
        {
            return RedirectToAction("Index", "Account");
        }

        CmConnectionHelper l_CmConnectionHelper = new CmConnectionHelper();
        string l_ConnectionString = l_CmConnectionHelper.Fnc_GetConnectionString();

        using (SqlConnection l_SqlConnection = new SqlConnection(l_ConnectionString))
        {
            l_SqlConnection.Open();
            SqlCommand l_SqlCommand = new SqlCommand();
            l_SqlCommand.Connection = l_SqlConnection;
            l_SqlCommand.CommandText = $" UPDATE TBU_Member SET Password = '{l_NewPassword}' WHERE PKGUID = '{l_ModLoggedInMember.PKGUID}' ; ";

            object l_Result = l_SqlCommand.ExecuteScalar();
            if (l_Result == DBNull.Value)
            {
                TempData["ErrorMessage"] = "Error in updating password.";
                return View("Index");
            }
        }

        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Account");
    }



    [HttpGet]
    public ActionResult Local()
    {
        ModMember? l_ModLoggedInMember = HttpContext.Session.Get<ModMember>("LoggedInMember");
        try
        {
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
                    return View("Index", l_ModLoggedInMember);
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
                l_SqlCommand.CommandText = $" SELECT * FROM TBU_Candidate WHERE Region = @mRegion AND NominatedFor = '{G_Local}' ORDER BY CandidateID ASC ";


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

            if (l_ListModCandidate_Local.Count == 0)
            {
                TempData["ErrorMessage"] = $"No local candidates are available for the {l_ModLoggedInMember.Region} region. You can only cast executive votes at the moment.";
                return View("Index", l_ModLoggedInMember);
            }

            ModCandidateDataset l_ModCandidateDataset = new ModCandidateDataset();
            l_ModCandidateDataset.ListModCandidate = l_ListModCandidate_Local;
            l_ModCandidateDataset.LoggedInMember = l_ModLoggedInMember;
            return View("Local", l_ModCandidateDataset);
        }
        catch (Exception ex)
        {
            new CmConnectionHelper().Vd_WriteToFile(ex.Message);
            TempData["ErrorMessage"] = ex.Message;
            return View("Index", l_ModLoggedInMember);
        }
    }
    [HttpPost]
    public ActionResult CastVoteLocal(ModCandidateDataset l_ModCandidateDataset)
    {
        try
        {
            List<ModCandidate> l_ListModCandidate_Local = new List<ModCandidate>();
            l_ListModCandidate_Local = l_ModCandidateDataset.ListModCandidate;

            ModMember? l_ModLoggedInMember = HttpContext.Session.Get<ModMember>("LoggedInMember");
            if (l_ModLoggedInMember == null)
            {
                return RedirectToAction("Index", "Account");
            }

            int l_TotalVoteCount = l_ListModCandidate_Local.Where(x => x.IsVote == true).Count();

            if (l_TotalVoteCount < l_ModLoggedInMember.LcCouncilSeats)
            {
                TempData["ErrorMessage"] = $"You have to select at least {l_ModLoggedInMember.LcCouncilSeats} local candidates.";
                l_ModCandidateDataset.LoggedInMember = l_ModLoggedInMember;
                return View("Local", l_ModCandidateDataset);
            }

            if (l_TotalVoteCount > l_ModLoggedInMember.LcCouncilSeats)
            {
                TempData["ErrorMessage"] = $"You can not select more than {l_ModLoggedInMember.LcCouncilSeats} local candidates.";
                l_ModCandidateDataset.LoggedInMember = l_ModLoggedInMember;
                return View("Local", l_ModCandidateDataset);
            }

            List<ModCandidate> l_ListModCandidate_Selected = new List<ModCandidate>();
            l_ListModCandidate_Selected = l_ListModCandidate_Local.Where(x => x.IsVote == true).ToList();

            return View("ConfirmationLocal", l_ListModCandidate_Selected);
        }
        catch (Exception ex)
        {
            new CmConnectionHelper().Vd_WriteToFile(ex.Message);
            TempData["ErrorMessage"] = ex.Message;
            return View("Index");
        }
    }
    [HttpPost]
    public ActionResult ConfirmationLocal(List<ModCandidate> l_ListModCandidate_Local)
    {
        try
        {
            ModMember? l_ModLoggedInMember = HttpContext.Session.Get<ModMember>("LoggedInMember");
            if (l_ModLoggedInMember == null)
            {
                return RedirectToAction("Index", "Account");
            }

            IEnumerableVote l_IEnumerableVote = new IEnumerableVote();
            foreach (ModCandidate l_ModCandidate in l_ListModCandidate_Local)
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

            int l_VoteCount_Executive;
            using (SqlConnection l_SqlConnection_Vote_Executive = new SqlConnection(l_ConnectionString))
            {
                l_SqlConnection_Vote_Executive.Open();
                SqlCommand l_SqlCommand_Vote_Executive = new SqlCommand();
                l_SqlCommand_Vote_Executive.Connection = l_SqlConnection_Vote_Executive;
                l_SqlCommand_Vote_Executive.Parameters.AddWithValue("@mMemberDID", l_ModLoggedInMember.PKGUID);
                l_SqlCommand_Vote_Executive.CommandText = $" SELECT COUNT(*) AS VoteCount FROM TBU_Vote WHERE MemberDID = @mMemberDID  AND NominatedFor = '{G_Executive}' ; ";

                object l_Result_Executive = l_SqlCommand_Vote_Executive.ExecuteScalar();
                if (l_Result_Executive == DBNull.Value)
                {
                    TempData["ErrorMessage"] = "Error in fetching casted executive vote count from database.";
                    return View("Index");
                }

                l_VoteCount_Executive = (int)l_Result_Executive;
            }

            int l_VoteCount_Local;
            using (SqlConnection l_SqlConnection_Vote_Local = new SqlConnection(l_ConnectionString))
            {
                l_SqlConnection_Vote_Local.Open();
                SqlCommand l_SqlCommand_Vote_Local = new SqlCommand();
                l_SqlCommand_Vote_Local.Connection = l_SqlConnection_Vote_Local;
                l_SqlCommand_Vote_Local.Parameters.AddWithValue("@mMemberDID", l_ModLoggedInMember.PKGUID);
                l_SqlCommand_Vote_Local.CommandText = $" SELECT COUNT(*) AS VoteCount FROM TBU_Vote WHERE MemberDID = @mMemberDID  AND NominatedFor = '{G_Local}' ; ";

                object l_Result_Local = l_SqlCommand_Vote_Local.ExecuteScalar();
                if (l_Result_Local == DBNull.Value)
                {
                    TempData["ErrorMessage"] = "Error in fetching casted local vote count from database.";
                    return View("Index");
                }

                l_VoteCount_Local = (int)l_Result_Local;
            }

            List<ModCandidate> l_ListModCandidate_Executive = new List<ModCandidate>();
            using (SqlConnection l_SqlConnection = new SqlConnection(l_ConnectionString))
            {
                l_SqlConnection.Open();
                SqlCommand l_SqlCommand = new SqlCommand();
                l_SqlCommand.Connection = l_SqlConnection;
                l_SqlCommand.Parameters.AddWithValue("@mRegion", l_ModLoggedInMember.Region);
                l_SqlCommand.CommandText = $" SELECT * FROM TBU_Candidate WHERE Region = @mRegion AND NominatedFor = '{G_Executive}' ORDER BY CandidateID ASC ";


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

            if (l_ListModCandidate_Executive.Count == 0)
            {
                if (l_VoteCount_Local > 0)
                {
                    return View("VotesCastedLogout");
                }
            }

            if (l_VoteCount_Local > 0 && l_VoteCount_Executive > 0)
            {
                return View("VotesCastedLogout");
            }
            else
            {
                return View("VotesCastedExecutive");
            }          
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
        ModMember? l_ModLoggedInMember = HttpContext.Session.Get<ModMember>("LoggedInMember");
        try
        {
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
                l_SqlCommand_Vote.CommandText = $" SELECT COUNT(*) AS VoteCount FROM TBU_Vote WHERE MemberDID = @mMemberDID  AND NominatedFor = '{G_Executive}'  ";

                object l_Result = l_SqlCommand_Vote.ExecuteScalar();
                if (l_Result == DBNull.Value)
                {
                    TempData["ErrorMessage"] = "Error in fetching casted vote count from database";
                    return View("Index", l_ModLoggedInMember);
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
                l_SqlCommand.CommandText = $" SELECT * FROM TBU_Candidate WHERE Region = @mRegion AND NominatedFor = '{G_Executive}' ORDER BY CandidateID ASC ";


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

            if (l_ListModCandidate_Executive.Count == 0)
            {
                TempData["ErrorMessage"] = $"No executive candidates are available for the {l_ModLoggedInMember.Region} region. You can only cast local votes at the moment.";
                return View("Index", l_ModLoggedInMember);
            }

            ModCandidateDataset l_ModCandidateDataset = new ModCandidateDataset();
            l_ModCandidateDataset.ListModCandidate = l_ListModCandidate_Executive;
            l_ModCandidateDataset.LoggedInMember = l_ModLoggedInMember;
            return View("Executive", l_ModCandidateDataset);
        }
        catch (Exception ex)
        {
            new CmConnectionHelper().Vd_WriteToFile(ex.Message);
            TempData["ErrorMessage"] = ex.Message;
            return View("Index", l_ModLoggedInMember);
        }
    }
    [HttpPost]
    public ActionResult CastVoteExecutive(ModCandidateDataset l_ModCandidateDataset)
    {
        try
        {
            List<ModCandidate> l_ListModCandidate_Executive = new List<ModCandidate>();
            l_ListModCandidate_Executive = l_ModCandidateDataset.ListModCandidate;

            ModMember? l_ModLoggedInMember = HttpContext.Session.Get<ModMember>("LoggedInMember");
            if (l_ModLoggedInMember == null)
            {
                return RedirectToAction("Index", "Account");
            }

            int l_TotalVoteCount = l_ListModCandidate_Executive.Where(x => x.IsVote == true).Count();

            if (l_TotalVoteCount < l_ModLoggedInMember.ExCouncilSeats)
            {
                TempData["ErrorMessage"] = $"You have to select at least {l_ModLoggedInMember.ExCouncilSeats} executive candidates.";
                l_ModCandidateDataset.LoggedInMember = l_ModLoggedInMember;
                return View("Executive", l_ModCandidateDataset);
            }

            if (l_TotalVoteCount > l_ModLoggedInMember.ExCouncilSeats)
            {
                TempData["ErrorMessage"] = $"You can not select more than {l_ModLoggedInMember.ExCouncilSeats} executive candidates.";
                l_ModCandidateDataset.LoggedInMember = l_ModLoggedInMember;
                return View("Executive", l_ModCandidateDataset);
            }

            List<ModCandidate> l_ListModCandidate_Selected = new List<ModCandidate>();
            l_ListModCandidate_Selected = l_ListModCandidate_Executive.Where(x => x.IsVote == true).ToList();

            return View("ConfirmationExecutive", l_ListModCandidate_Selected);
        }
        catch (Exception ex)
        {
            new CmConnectionHelper().Vd_WriteToFile(ex.Message);
            TempData["ErrorMessage"] = ex.Message;
            return View("Index");
        }
    }
    [HttpPost]
    public ActionResult ConfirmationExecutive(List<ModCandidate> l_ListModCandidate_Executive)
    {
        try
        {
            ModMember? l_ModLoggedInMember = HttpContext.Session.Get<ModMember>("LoggedInMember");
            if (l_ModLoggedInMember == null)
            {
                return RedirectToAction("Index", "Account");
            }

            IEnumerableVote l_IEnumerableVote = new IEnumerableVote();
            foreach (ModCandidate l_ModCandidate in l_ListModCandidate_Executive)
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

            int l_VoteCount_Executive;
            using (SqlConnection l_SqlConnection_Vote_Executive = new SqlConnection(l_ConnectionString))
            {
                l_SqlConnection_Vote_Executive.Open();
                SqlCommand l_SqlCommand_Vote_Executive = new SqlCommand();
                l_SqlCommand_Vote_Executive.Connection = l_SqlConnection_Vote_Executive;
                l_SqlCommand_Vote_Executive.Parameters.AddWithValue("@mMemberDID", l_ModLoggedInMember.PKGUID);
                l_SqlCommand_Vote_Executive.CommandText = $" SELECT COUNT(*) AS VoteCount FROM TBU_Vote WHERE MemberDID = @mMemberDID  AND NominatedFor = '{G_Executive}' ; ";

                object l_Result_Executive = l_SqlCommand_Vote_Executive.ExecuteScalar();
                if (l_Result_Executive == DBNull.Value)
                {
                    TempData["ErrorMessage"] = "Error in fetching casted executive vote count from database.";
                    return View("Index");
                }

                l_VoteCount_Executive = (int)l_Result_Executive;
            }

            int l_VoteCount_Local;
            using (SqlConnection l_SqlConnection_Vote_Local = new SqlConnection(l_ConnectionString))
            {
                l_SqlConnection_Vote_Local.Open();
                SqlCommand l_SqlCommand_Vote_Local = new SqlCommand();
                l_SqlCommand_Vote_Local.Connection = l_SqlConnection_Vote_Local;
                l_SqlCommand_Vote_Local.Parameters.AddWithValue("@mMemberDID", l_ModLoggedInMember.PKGUID);
                l_SqlCommand_Vote_Local.CommandText = $" SELECT COUNT(*) AS VoteCount FROM TBU_Vote WHERE MemberDID = @mMemberDID  AND NominatedFor = '{G_Local}' ; ";

                object l_Result_Local = l_SqlCommand_Vote_Local.ExecuteScalar();
                if (l_Result_Local == DBNull.Value)
                {
                    TempData["ErrorMessage"] = "Error in fetching casted local vote count from database.";
                    return View("Index");
                }

                l_VoteCount_Local = (int)l_Result_Local;
            }

            List<ModCandidate> l_ListModCandidate_Local = new List<ModCandidate>();
            using (SqlConnection l_SqlConnection = new SqlConnection(l_ConnectionString))
            {
                l_SqlConnection.Open();
                SqlCommand l_SqlCommand = new SqlCommand();
                l_SqlCommand.Connection = l_SqlConnection;
                l_SqlCommand.Parameters.AddWithValue("@mRegion", l_ModLoggedInMember.Region);
                l_SqlCommand.CommandText = $" SELECT * FROM TBU_Candidate WHERE Region = @mRegion AND NominatedFor = '{G_Local}' ORDER BY CandidateID ASC ";


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

            if (l_ListModCandidate_Local.Count == 0)
            {
                if (l_VoteCount_Executive > 0)
                {
                    return View("VotesCastedLogout");
                }
            }

            if (l_VoteCount_Local > 0 && l_VoteCount_Executive > 0)
            {
                return View("VotesCastedLogout");
            }
            else
            {
                return View("VotesCastedLocal");
            }

        }
        catch (Exception ex)
        {
            new CmConnectionHelper().Vd_WriteToFile(ex.Message);
            TempData["ErrorMessage"] = ex.Message;
            return View("Index");
        }
    }
}