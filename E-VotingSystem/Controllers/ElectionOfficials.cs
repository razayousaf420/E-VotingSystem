using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;

public class ElectionOfficials : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    //Pending
    [HttpPost]
    public ActionResult Login(ModElectionOfficial l_ModElectionOfficial)
    {
        try
        {
            CmConnectionHelper l_CmConnectionHelper = new CmConnectionHelper();
            string l_ConnectionString = l_CmConnectionHelper.Fnc_GetConnectionString();

            using (SqlConnection l_SqlConnection = new SqlConnection(l_ConnectionString))
            {
                l_SqlConnection.Open();
                SqlCommand l_SqlCommand = new SqlCommand();
                l_SqlCommand.Connection = l_SqlConnection;
                l_SqlCommand.Parameters.AddWithValue("@mElectionOfficialID", l_ModElectionOfficial.ElectionOfficialID?.Trim());
                l_SqlCommand.Parameters.AddWithValue("@mPassword", l_ModElectionOfficial.Password?.Trim());
                l_SqlCommand.CommandText = " SELECT * FROM TBU_ElectionOfficial WHERE ElectionOfficialID = @mElectionOfficialID AND Password = @mPassword ";

                ModElectionOfficial l_LoggedInElectionOfficial = new ModElectionOfficial();
                SqlDataReader l_SqlDataReader = l_SqlCommand.ExecuteReader();
                if (l_SqlDataReader.Read() == true)
                {
                    l_LoggedInElectionOfficial.PKGUID = (Guid)l_SqlDataReader["PKGUID"];
                    l_LoggedInElectionOfficial.ElectionOfficialID = (string)l_SqlDataReader["ElectionOfficialID"];
                    l_LoggedInElectionOfficial.Password = (string)l_SqlDataReader["Password"];
                    l_LoggedInElectionOfficial.ElectionOfficialName = (string)l_SqlDataReader["ElectionOfficialName"];
                    l_LoggedInElectionOfficial.Region = (string)l_SqlDataReader["Region"];
                    l_LoggedInElectionOfficial.Email = (string)l_SqlDataReader["Email"];
                    l_LoggedInElectionOfficial.ContactNo = (string)l_SqlDataReader["ContactNo"];
                    l_LoggedInElectionOfficial.Mobile = (string)l_SqlDataReader["Mobile"];
                    l_LoggedInElectionOfficial.Address = (string)l_SqlDataReader["Address"];
                }
                else
                {
                    return View("ErrorLogin");
                }

                if (string.IsNullOrWhiteSpace(l_LoggedInElectionOfficial.Mobile))
                {
                    return View("ErrorMobile");
                }

                HttpContext.Session.Set<ModElectionOfficial>("LoggedInElectionOfficial", l_LoggedInElectionOfficial);
                return RedirectToAction("IndexOfficial", "OTP", l_LoggedInElectionOfficial);             
                //return RedirectToAction("Dashboard", "ElectionOfficials");
            }
        }
        catch (Exception ex)
        {
            new CmConnectionHelper().Vd_WriteToFile(ex.Message);
            TempData["ErrorMessage"] = ex.Message;
            return View("Index");
        }
    }

    [HttpPost]
    public ActionResult Logout()
    {
        try
        {
            HttpContext.Session.Clear();
            return View("Index");
        }
        catch (Exception ex)
        {
            new CmConnectionHelper().Vd_WriteToFile(ex.Message);
            TempData["ErrorMessage"] = ex.Message;
            return View("Index");
        }
    }

    [HttpGet]
    public ActionResult Dashboard()
    {
        try
        {
            ModElectionOfficial? l_LoggedInModElectionOfficial = HttpContext.Session.Get<ModElectionOfficial>("LoggedInElectionOfficial");
            if (l_LoggedInModElectionOfficial == null)
            {
                return View("Index");
            }

            return View();
        }
        catch (Exception ex)
        {
            new CmConnectionHelper().Vd_WriteToFile(ex.Message);
            TempData["ErrorMessage"] = ex.Message;
            return View("Index");
        }
    }

    [HttpPost]
    public ActionResult CheckStatus(string MemberID)
    {
        try
        {
            CmConnectionHelper l_CmConnectionHelper = new CmConnectionHelper();
            string l_ConnectionString = l_CmConnectionHelper.Fnc_GetConnectionString();

            using (SqlConnection l_SqlConnection_Member = new SqlConnection(l_ConnectionString))
            {
                l_SqlConnection_Member.Open();
                SqlCommand l_SqlCommand_Member = new SqlCommand();
                l_SqlCommand_Member.Connection = l_SqlConnection_Member;
                l_SqlCommand_Member.Parameters.AddWithValue("@mMemberID", MemberID.Trim());
                l_SqlCommand_Member.CommandText = " SELECT * FROM Vw_TBU_Member WHERE MemberID = @mMemberID ";

                ModMember l_ModLoggedInMember = new ModMember();
                SqlDataReader l_SqlDataReader_Member = l_SqlCommand_Member.ExecuteReader();
                if (l_SqlDataReader_Member.Read() == true)
                {
                    l_ModLoggedInMember.PKGUID = (Guid)l_SqlDataReader_Member["PKGUID"];
                    l_ModLoggedInMember.MemberID = (string)l_SqlDataReader_Member["MemberID"];
                    l_ModLoggedInMember.Password = (string)l_SqlDataReader_Member["Password"];
                    l_ModLoggedInMember.MemberName = (string)l_SqlDataReader_Member["MemberName"];
                    l_ModLoggedInMember.Region = (string)l_SqlDataReader_Member["Region"];
                    l_ModLoggedInMember.Email = (string)l_SqlDataReader_Member["Email"];
                    l_ModLoggedInMember.ContactNo = (string)l_SqlDataReader_Member["ContactNo"];
                    l_ModLoggedInMember.Mobile = (string)l_SqlDataReader_Member["Mobile"];
                    l_ModLoggedInMember.Address = (string)l_SqlDataReader_Member["Address"];
                    l_ModLoggedInMember.LcCouncilSeats = (int)l_SqlDataReader_Member["LcCouncilSeats"];
                    l_ModLoggedInMember.ExCouncilSeats = (int)l_SqlDataReader_Member["ExCouncilSeats"];
                }
                else
                {
                    TempData["ErrorMessage"] = "Invalid Member ID";
                    return View("Dashboard");
                }

                using (SqlConnection l_SqlConnection_Vote = new SqlConnection(l_ConnectionString))
                {
                    l_SqlConnection_Vote.Open();
                    SqlCommand l_SqlCommand_Vote = new SqlCommand();
                    l_SqlCommand_Vote.Connection = l_SqlConnection_Vote;
                    l_SqlCommand_Vote.Parameters.AddWithValue("@mMemberDID", l_ModLoggedInMember.PKGUID);
                    l_SqlCommand_Vote.CommandText = " SELECT COUNT(*) AS VoteCount FROM TBU_Vote WHERE MemberDID = @mMemberDID; ";

                    object l_Result = l_SqlCommand_Vote.ExecuteScalar();
                    if (l_Result == DBNull.Value)
                    {
                        TempData["ErrorMessage"] = "Error in fetching casted vote count from database";
                        return View("Dashboard");
                    }

                    int l_TotalVoteCount = l_ModLoggedInMember.LcCouncilSeats + l_ModLoggedInMember.ExCouncilSeats;
                    int l_VoteCount = (int)l_Result;

                    if (l_VoteCount > 0)
                    {
                        TempData["ErrorMessage"] = $"{l_ModLoggedInMember.MemberName} has already casted {l_VoteCount} out of {l_TotalVoteCount} votes.";
                        return View("Dashboard");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = $"No votes casted by {l_ModLoggedInMember.MemberName}.";
                        return View("Dashboard");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            new CmConnectionHelper().Vd_WriteToFile(ex.Message);
            TempData["ErrorMessage"] = ex.Message;
            return View("Dashboard");
        }
    }

    [HttpPost]
    public ActionResult Enable(string MemberID)
    {
        try
        {
            ModElectionOfficial? l_LoggedInModElectionOfficial = HttpContext.Session.Get<ModElectionOfficial>("LoggedInElectionOfficial");
            if (l_LoggedInModElectionOfficial == null)
            {
                return View("Index");
            }

            CmConnectionHelper l_CmConnectionHelper = new CmConnectionHelper();
            string l_ConnectionString = l_CmConnectionHelper.Fnc_GetConnectionString();

            ModMember l_ModLoggedInMember = new ModMember();
            using (SqlConnection l_SqlConnection_Member = new SqlConnection(l_ConnectionString))
            {
                l_SqlConnection_Member.Open();
                SqlCommand l_SqlCommand_Member = new SqlCommand();
                l_SqlCommand_Member.Connection = l_SqlConnection_Member;
                l_SqlCommand_Member.Parameters.AddWithValue("@mMemberID", MemberID.Trim());
                l_SqlCommand_Member.CommandText = " SELECT * FROM Vw_TBU_Member WHERE MemberID = @mMemberID ";

                SqlDataReader l_SqlDataReader_Member = l_SqlCommand_Member.ExecuteReader();
                if (l_SqlDataReader_Member.Read() == true)
                {
                    l_ModLoggedInMember.PKGUID = (Guid)l_SqlDataReader_Member["PKGUID"];
                    l_ModLoggedInMember.MemberID = (string)l_SqlDataReader_Member["MemberID"];
                    l_ModLoggedInMember.Password = (string)l_SqlDataReader_Member["Password"];
                    l_ModLoggedInMember.MemberName = (string)l_SqlDataReader_Member["MemberName"];
                    l_ModLoggedInMember.Region = (string)l_SqlDataReader_Member["Region"];
                    l_ModLoggedInMember.Email = (string)l_SqlDataReader_Member["Email"];
                    l_ModLoggedInMember.ContactNo = (string)l_SqlDataReader_Member["ContactNo"];
                    l_ModLoggedInMember.Mobile = (string)l_SqlDataReader_Member["Mobile"];
                    l_ModLoggedInMember.Address = (string)l_SqlDataReader_Member["Address"];
                    l_ModLoggedInMember.LcCouncilSeats = (int)l_SqlDataReader_Member["LcCouncilSeats"];
                    l_ModLoggedInMember.ExCouncilSeats = (int)l_SqlDataReader_Member["ExCouncilSeats"];
                }
                else
                {
                    TempData["ErrorMessage"] = "Invalid Member ID";
                    return View("Dashboard");
                }

                if (l_LoggedInModElectionOfficial.Region?.ToUpper() != l_ModLoggedInMember.Region.ToUpper())
                {
                    TempData["ErrorMessage"] = "You cannot enable member of another region.";
                    return View("Dashboard");
                }
            }

            using (SqlConnection l_SqlConnection_Vote = new SqlConnection(l_ConnectionString))
            {
                l_SqlConnection_Vote.Open();
                SqlCommand l_SqlCommand_Vote = new SqlCommand();
                l_SqlCommand_Vote.Connection = l_SqlConnection_Vote;
                l_SqlCommand_Vote.Parameters.AddWithValue("@mMemberDID", l_ModLoggedInMember.PKGUID);
                l_SqlCommand_Vote.CommandText = " SELECT COUNT(*) AS VoteCount FROM TBU_Vote WHERE MemberDID = @mMemberDID; ";

                object l_Result = l_SqlCommand_Vote.ExecuteScalar();
                if (l_Result == DBNull.Value)
                {
                    TempData["ErrorMessage"] = "Error in fetching casted vote count from database";
                    return View("Dashboard");
                }

                int l_TotalVoteCount = l_ModLoggedInMember.LcCouncilSeats + l_ModLoggedInMember.ExCouncilSeats;
                int l_VoteCount = (int)l_Result;

                if (l_VoteCount > 0)
                {
                    TempData["ErrorMessage"] = $"{l_ModLoggedInMember.MemberName} has already casted {l_VoteCount} votes. You cannot enable him.";
                    return View("Dashboard");
                }
            }

            using (SqlConnection l_SqlConnection = new SqlConnection(l_ConnectionString))
            {
                l_SqlConnection.Open();
                SqlCommand l_SqlCommand = new SqlCommand();
                l_SqlCommand.Connection = l_SqlConnection;
                l_SqlCommand.Parameters.AddWithValue("@mMemberID", MemberID.Trim());
                l_SqlCommand.CommandText = " UPDATE TBU_Member SET IsEnabled = 1 WHERE MemberID = @mMemberID ";

                l_SqlCommand.ExecuteNonQuery();
            }

            TempData["ErrorMessage"] = "Member has been enabled.";
            return View("Dashboard");
        }

        catch (Exception ex)
        {
            new CmConnectionHelper().Vd_WriteToFile(ex.Message);
            TempData["ErrorMessage"] = ex.Message;
            return View("Dashboard");
        }
    }

    [HttpPost]
    public ActionResult Disable(string MemberID)
    {
        try
        {
            ModElectionOfficial? l_LoggedInModElectionOfficial = HttpContext.Session.Get<ModElectionOfficial>("LoggedInElectionOfficial");
            if (l_LoggedInModElectionOfficial == null)
            {
                return View("Index");
            }

            CmConnectionHelper l_CmConnectionHelper = new CmConnectionHelper();
            string l_ConnectionString = l_CmConnectionHelper.Fnc_GetConnectionString();

            ModMember l_ModLoggedInMember = new ModMember();
            using (SqlConnection l_SqlConnection_Member = new SqlConnection(l_ConnectionString))
            {
                l_SqlConnection_Member.Open();
                SqlCommand l_SqlCommand_Member = new SqlCommand();
                l_SqlCommand_Member.Connection = l_SqlConnection_Member;
                l_SqlCommand_Member.Parameters.AddWithValue("@mMemberID", MemberID.Trim());
                l_SqlCommand_Member.CommandText = " SELECT * FROM Vw_TBU_Member WHERE MemberID = @mMemberID ";

                SqlDataReader l_SqlDataReader_Member = l_SqlCommand_Member.ExecuteReader();
                if (l_SqlDataReader_Member.Read() == true)
                {
                    l_ModLoggedInMember.PKGUID = (Guid)l_SqlDataReader_Member["PKGUID"];
                    l_ModLoggedInMember.MemberID = (string)l_SqlDataReader_Member["MemberID"];
                    l_ModLoggedInMember.Password = (string)l_SqlDataReader_Member["Password"];
                    l_ModLoggedInMember.MemberName = (string)l_SqlDataReader_Member["MemberName"];
                    l_ModLoggedInMember.Region = (string)l_SqlDataReader_Member["Region"];
                    l_ModLoggedInMember.Email = (string)l_SqlDataReader_Member["Email"];
                    l_ModLoggedInMember.ContactNo = (string)l_SqlDataReader_Member["ContactNo"];
                    l_ModLoggedInMember.Mobile = (string)l_SqlDataReader_Member["Mobile"];
                    l_ModLoggedInMember.Address = (string)l_SqlDataReader_Member["Address"];
                    l_ModLoggedInMember.LcCouncilSeats = (int)l_SqlDataReader_Member["LcCouncilSeats"];
                    l_ModLoggedInMember.ExCouncilSeats = (int)l_SqlDataReader_Member["ExCouncilSeats"];
                }
                else
                {
                    TempData["ErrorMessage"] = "Invalid Member ID";
                    return View("Dashboard");
                }

                if (l_LoggedInModElectionOfficial.Region?.ToUpper() != l_ModLoggedInMember.Region.ToUpper())
                {
                    TempData["ErrorMessage"] = "You cannot enable member of another region.";
                    return View("Dashboard");
                }
            }

            using (SqlConnection l_SqlConnection_Vote = new SqlConnection(l_ConnectionString))
            {
                l_SqlConnection_Vote.Open();
                SqlCommand l_SqlCommand_Vote = new SqlCommand();
                l_SqlCommand_Vote.Connection = l_SqlConnection_Vote;
                l_SqlCommand_Vote.Parameters.AddWithValue("@mMemberDID", l_ModLoggedInMember.PKGUID);
                l_SqlCommand_Vote.CommandText = " SELECT COUNT(*) AS VoteCount FROM TBU_Vote WHERE MemberDID = @mMemberDID; ";

                object l_Result = l_SqlCommand_Vote.ExecuteScalar();
                if (l_Result == DBNull.Value)
                {
                    TempData["ErrorMessage"] = "Error in fetching casted vote count from database";
                    return View("Dashboard");
                }

                int l_TotalVoteCount = l_ModLoggedInMember.LcCouncilSeats + l_ModLoggedInMember.ExCouncilSeats;
                int l_VoteCount = (int)l_Result;

                if (l_VoteCount > 0)
                {
                    TempData["ErrorMessage"] = $"{l_ModLoggedInMember.MemberName} has already casted {l_VoteCount} votes. You cannot disable him.";
                    return View("Dashboard");
                }
            }

            using (SqlConnection l_SqlConnection = new SqlConnection(l_ConnectionString))
            {
                l_SqlConnection.Open();
                SqlCommand l_SqlCommand = new SqlCommand();
                l_SqlCommand.Connection = l_SqlConnection;
                l_SqlCommand.Parameters.AddWithValue("@mMemberID", MemberID.Trim());
                l_SqlCommand.CommandText = " UPDATE TBU_Member SET IsEnabled = 0 WHERE MemberID = @mMemberID ";

                l_SqlCommand.ExecuteNonQuery();
            }

            TempData["ErrorMessage"] = "Member has been disabled.";
            return View("Dashboard");
        }
        catch (Exception ex)
        {
            new CmConnectionHelper().Vd_WriteToFile(ex.Message);
            TempData["ErrorMessage"] = ex.Message;
            return View("Dashboard");
        }
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

        ModElectionOfficial? l_ModLoggedInElectionOfficial = HttpContext.Session.Get<ModElectionOfficial>("LoggedInElectionOfficial");
        if (l_ModLoggedInElectionOfficial == null)
        {
            return RedirectToAction("Index", "ElectionOfficials");
        }

        CmConnectionHelper l_CmConnectionHelper = new CmConnectionHelper();
        string l_ConnectionString = l_CmConnectionHelper.Fnc_GetConnectionString();

        using (SqlConnection l_SqlConnection = new SqlConnection(l_ConnectionString))
        {
            l_SqlConnection.Open();
            SqlCommand l_SqlCommand = new SqlCommand();
            l_SqlCommand.Connection = l_SqlConnection;
            l_SqlCommand.CommandText = $" UPDATE TBU_ElectionOfficial SET Password = '{l_NewPassword}' WHERE PKGUID = '{l_ModLoggedInElectionOfficial.PKGUID}' ; ";

            object l_Result = l_SqlCommand.ExecuteScalar();
            if (l_Result == DBNull.Value)
            {
                TempData["ErrorMessage"] = "Error in updating password.";
                return View("Index");
            }
        }

        HttpContext.Session.Clear();
        return RedirectToAction("Index", "ElectionOfficials");
    }

















    //OK
    [HttpGet]
    public ActionResult CandidateVoteInfo()
    {
        try
        {
            ModElectionOfficial? l_LoggedInElectionOfficial = HttpContext.Session.Get<ModElectionOfficial>("LoggedInElectionOfficial");
            if (l_LoggedInElectionOfficial == null)
            {
                return View("Index");
            }


            ModCanidateResultDataSet l_ModCanidateResultDataSet = new ModCanidateResultDataSet();
            CmConnectionHelper l_CmConnectionHelper = new CmConnectionHelper();
            string l_ConnectionString = l_CmConnectionHelper.Fnc_GetConnectionString();

            using (SqlConnection l_SqlConnection_Local = new SqlConnection(l_ConnectionString))
            {
                l_SqlConnection_Local.Open();
                SqlCommand l_SqlCommand_Local = new SqlCommand("RptUI_Candidate_Result_Local", l_SqlConnection_Local);
                l_SqlCommand_Local.Parameters.AddWithValue("@mRegionID", l_LoggedInElectionOfficial.Region);
                l_SqlCommand_Local.CommandType = CommandType.StoredProcedure;

                List<ModRptCandidateResult> l_ListModRptCandidateResult_Local = new List<ModRptCandidateResult>();

                Int64 i = 1;
                SqlDataReader l_SqlDataReader_Local = l_SqlCommand_Local.ExecuteReader();
                while (l_SqlDataReader_Local.Read() == true)
                {
                    ModRptCandidateResult l_ModRptCandidateResult_Local = new ModRptCandidateResult();
                    l_ModRptCandidateResult_Local.SrNo = i;
                    l_ModRptCandidateResult_Local.PKGUID = (Guid)l_SqlDataReader_Local["PKGUID"];
                    l_ModRptCandidateResult_Local.CandidateID = (string)l_SqlDataReader_Local["CandidateID"];
                    l_ModRptCandidateResult_Local.CandidateName = (string)l_SqlDataReader_Local["CandidateName"];
                    l_ModRptCandidateResult_Local.Region = (string)l_SqlDataReader_Local["Region"];
                    l_ModRptCandidateResult_Local.VoteCount = (int)l_SqlDataReader_Local["VoteCount"];
                    l_ModRptCandidateResult_Local.Image = (string)l_SqlDataReader_Local["Image"];

                    l_ListModRptCandidateResult_Local.Add(l_ModRptCandidateResult_Local);
                    i++;
                }

                l_ModCanidateResultDataSet.ListModRptCandidateResult_Local = l_ListModRptCandidateResult_Local;
            }

            using (SqlConnection l_SqlConnection_Executive = new SqlConnection(l_ConnectionString))
            {
                l_SqlConnection_Executive.Open();
                SqlCommand l_SqlCommand_Executive = new SqlCommand("RptUI_Candidate_Result_Executive", l_SqlConnection_Executive);
                l_SqlCommand_Executive.Parameters.AddWithValue("@mRegionID", l_LoggedInElectionOfficial.Region);
                l_SqlCommand_Executive.CommandType = CommandType.StoredProcedure;

                List<ModRptCandidateResult> l_ListModRptCandidateResult_Executive = new List<ModRptCandidateResult>();

                Int64 i = 1;
                SqlDataReader l_SqlDataReader_Executive = l_SqlCommand_Executive.ExecuteReader();
                while (l_SqlDataReader_Executive.Read() == true)
                {
                    ModRptCandidateResult l_ModRptCandidateResult_Executive = new ModRptCandidateResult();
                    l_ModRptCandidateResult_Executive.SrNo = i;
                    l_ModRptCandidateResult_Executive.PKGUID = (Guid)l_SqlDataReader_Executive["PKGUID"];
                    l_ModRptCandidateResult_Executive.CandidateID = (string)l_SqlDataReader_Executive["CandidateID"];
                    l_ModRptCandidateResult_Executive.CandidateName = (string)l_SqlDataReader_Executive["CandidateName"];
                    l_ModRptCandidateResult_Executive.Region = (string)l_SqlDataReader_Executive["Region"];
                    l_ModRptCandidateResult_Executive.VoteCount = (int)l_SqlDataReader_Executive["VoteCount"];
                    l_ModRptCandidateResult_Executive.Image = (string)l_SqlDataReader_Executive["Image"];

                    l_ListModRptCandidateResult_Executive.Add(l_ModRptCandidateResult_Executive);
                    i++;
                }

                l_ModCanidateResultDataSet.ListModRptCandidateResult_Executive = l_ListModRptCandidateResult_Executive;
            }

            return View("CandidateVoteInfo", l_ModCanidateResultDataSet);
        }
        catch (Exception ex)
        {
            new CmConnectionHelper().Vd_WriteToFile(ex.Message);
            TempData["ErrorMessage"] = ex.Message;
            return View("Index");
        }
    }

    //OK
    [HttpGet]
    public ActionResult CandidateVoteInfoDetailExecutive(string PKGUID)
    {
        try
        {
            Guid l_CandidateDID = Guid.Parse(PKGUID);

            CmConnectionHelper l_CmConnectionHelper = new CmConnectionHelper();
            string l_ConnectionString = l_CmConnectionHelper.Fnc_GetConnectionString();

            List<ModRptCandidateResultDetail> l_ListModRptCandidateResultDetail = new List<ModRptCandidateResultDetail>();

            using (SqlConnection l_SqlConnection_Local = new SqlConnection(l_ConnectionString))
            {
                l_SqlConnection_Local.Open();
                SqlCommand l_SqlCommand_Local = new SqlCommand("RptUI_Candidate_Result_Detail_Executive", l_SqlConnection_Local);
                l_SqlCommand_Local.Parameters.AddWithValue("@mPKGUID", l_CandidateDID);
                l_SqlCommand_Local.CommandType = CommandType.StoredProcedure;

                SqlDataReader l_SqlDataReader_Local = l_SqlCommand_Local.ExecuteReader();
                while (l_SqlDataReader_Local.Read() == true)
                {
                    ModRptCandidateResultDetail l_ModRptCandidateResultDetail = new ModRptCandidateResultDetail();
                    l_ModRptCandidateResultDetail.SrNo = (Int64)l_SqlDataReader_Local["SrNo"];
                    l_ModRptCandidateResultDetail.MemberID = (string)l_SqlDataReader_Local["MemberID"];
                    l_ModRptCandidateResultDetail.MemberName = (string)l_SqlDataReader_Local["MemberName"];
                    l_ModRptCandidateResultDetail.Mobile = (string)l_SqlDataReader_Local["Mobile"];
                    l_ModRptCandidateResultDetail.Region = (string)l_SqlDataReader_Local["Region"];
                    l_ModRptCandidateResultDetail.VoteDate = (string)l_SqlDataReader_Local["VoteDate"];
                    l_ModRptCandidateResultDetail.VoteTime = (string)l_SqlDataReader_Local["VoteTime"];

                    l_ListModRptCandidateResultDetail.Add(l_ModRptCandidateResultDetail);
                }
            }

            return View("CandidateVoteInfoDetail", l_ListModRptCandidateResultDetail);
        }
        catch (Exception ex)
        {
            new CmConnectionHelper().Vd_WriteToFile(ex.Message);
            TempData["ErrorMessage"] = ex.Message;
            return View("Index");
        }
    }

    //OK
    [HttpGet]
    public ActionResult CandidateVoteInfoDetailLocal(string PKGUID)
    {
        try
        {
            Guid l_CandidateDID = Guid.Parse(PKGUID);

            CmConnectionHelper l_CmConnectionHelper = new CmConnectionHelper();
            string l_ConnectionString = l_CmConnectionHelper.Fnc_GetConnectionString();

            List<ModRptCandidateResultDetail> l_ListModRptCandidateResultDetail = new List<ModRptCandidateResultDetail>();

            using (SqlConnection l_SqlConnection_Local = new SqlConnection(l_ConnectionString))
            {
                l_SqlConnection_Local.Open();
                SqlCommand l_SqlCommand_Local = new SqlCommand("RptUI_Candidate_Result_Detail_Local", l_SqlConnection_Local);
                l_SqlCommand_Local.Parameters.AddWithValue("@mPKGUID", l_CandidateDID);
                l_SqlCommand_Local.CommandType = CommandType.StoredProcedure;

                SqlDataReader l_SqlDataReader_Local = l_SqlCommand_Local.ExecuteReader();
                while (l_SqlDataReader_Local.Read() == true)
                {
                    ModRptCandidateResultDetail l_ModRptCandidateResultDetail = new ModRptCandidateResultDetail();
                    l_ModRptCandidateResultDetail.SrNo = (Int64)l_SqlDataReader_Local["SrNo"];
                    l_ModRptCandidateResultDetail.MemberID = (string)l_SqlDataReader_Local["MemberID"];
                    l_ModRptCandidateResultDetail.MemberName = (string)l_SqlDataReader_Local["MemberName"];
                    l_ModRptCandidateResultDetail.Mobile = (string)l_SqlDataReader_Local["Mobile"];
                    l_ModRptCandidateResultDetail.Region = (string)l_SqlDataReader_Local["Region"];
                    l_ModRptCandidateResultDetail.VoteDate = (string)l_SqlDataReader_Local["VoteDate"];
                    l_ModRptCandidateResultDetail.VoteTime = (string)l_SqlDataReader_Local["VoteTime"];

                    l_ListModRptCandidateResultDetail.Add(l_ModRptCandidateResultDetail);
                }
            }

            return View("CandidateVoteInfoDetail", l_ListModRptCandidateResultDetail);
        }
        catch (Exception ex)
        {
            new CmConnectionHelper().Vd_WriteToFile(ex.Message);
            TempData["ErrorMessage"] = ex.Message;
            return View("Index");
        }
    }
}