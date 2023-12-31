﻿using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;

public class ElectionCommissionerController : Controller
{
    [HttpGet]
    public ActionResult Index()
    {
        return View();
    }

    //Pending
    [HttpPost]
    public ActionResult Login(ModElectionCommissioner l_ModElectionCommissioner)
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
                l_SqlCommand.Parameters.AddWithValue("@mElectionCommissionerID", l_ModElectionCommissioner.ElectionCommissionerID?.Trim());
                l_SqlCommand.Parameters.AddWithValue("@mPassword", l_ModElectionCommissioner.Password?.Trim());
                l_SqlCommand.CommandText = " SELECT * FROM TBU_ElectionCommissioner WHERE ElectionCommissionerID = @mElectionCommissionerID AND Password = @mPassword ";

                ModElectionCommissioner l_LoggedInElectionCommissioner = new ModElectionCommissioner();
                SqlDataReader l_SqlDataReader = l_SqlCommand.ExecuteReader();
                if (l_SqlDataReader.Read() == true)
                {
                    l_LoggedInElectionCommissioner.PKGUID = (Guid)l_SqlDataReader["PKGUID"];
                    l_LoggedInElectionCommissioner.ElectionCommissionerID = (string)l_SqlDataReader["ElectionCommissionerID"];
                    l_LoggedInElectionCommissioner.Password = (string)l_SqlDataReader["Password"];
                    l_LoggedInElectionCommissioner.ElectionCommissionerName = (string)l_SqlDataReader["ElectionCommissionerName"];
                    l_LoggedInElectionCommissioner.Region = (string)l_SqlDataReader["Region"];
                    l_LoggedInElectionCommissioner.Email = (string)l_SqlDataReader["Email"];
                    l_LoggedInElectionCommissioner.ContactNo = (string)l_SqlDataReader["ContactNo"];
                    l_LoggedInElectionCommissioner.Mobile = (string)l_SqlDataReader["Mobile"];
                    l_LoggedInElectionCommissioner.Address = (string)l_SqlDataReader["Address"];
                }
                else
                {
                    return View("ErrorLogin");
                }

                if (string.IsNullOrWhiteSpace(l_LoggedInElectionCommissioner.Mobile))
                {
                    return View("ErrorMobile");
                }

                HttpContext.Session.Set<ModElectionCommissioner>("LoggedInElectionCommissioner", l_LoggedInElectionCommissioner);
                return RedirectToAction("IndexCommissioner", "OTP", l_LoggedInElectionCommissioner);
                //return RedirectToAction("Dashboard", "ElectionCommissioner");
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
            ModElectionCommissioner? l_LoggedInElectionCommissioner = HttpContext.Session.Get<ModElectionCommissioner>("LoggedInElectionCommissioner");
            if (l_LoggedInElectionCommissioner == null)
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

        ModElectionCommissioner? l_ModLoggedInElectionCommissioner = HttpContext.Session.Get<ModElectionCommissioner>("LoggedInElectionCommissioner");
        if (l_ModLoggedInElectionCommissioner == null)
        {
            return RedirectToAction("Index", "ElectionCommissioner");
        }

        CmConnectionHelper l_CmConnectionHelper = new CmConnectionHelper();
        string l_ConnectionString = l_CmConnectionHelper.Fnc_GetConnectionString();

        using (SqlConnection l_SqlConnection = new SqlConnection(l_ConnectionString))
        {
            l_SqlConnection.Open();
            SqlCommand l_SqlCommand = new SqlCommand();
            l_SqlCommand.Connection = l_SqlConnection;
            l_SqlCommand.CommandText = $" UPDATE TBU_ElectionCommissioner SET Password = '{l_NewPassword}' WHERE PKGUID = '{l_ModLoggedInElectionCommissioner.PKGUID}' ; ";

            object l_Result = l_SqlCommand.ExecuteScalar();
            if (l_Result == DBNull.Value)
            {
                TempData["ErrorMessage"] = "Error in updating password.";
                return View("Index");
            }
        }

        HttpContext.Session.Clear();
        return RedirectToAction("Index", "ElectionCommissioner");
    }





    //OK
    [HttpGet]
    public ActionResult CandidateVoteInfoLahore()
    {
        try
        {
            ModElectionCommissioner? l_LoggedInElectionCommissioner = HttpContext.Session.Get<ModElectionCommissioner>("LoggedInElectionCommissioner");
            if (l_LoggedInElectionCommissioner == null)
            {
                return View("Dashboard");
            }


            ModCanidateResultDataSet l_ModCanidateResultDataSet = new ModCanidateResultDataSet();
            CmConnectionHelper l_CmConnectionHelper = new CmConnectionHelper();
            string l_ConnectionString = l_CmConnectionHelper.Fnc_GetConnectionString();

            using (SqlConnection l_SqlConnection_Local = new SqlConnection(l_ConnectionString))
            {
                l_SqlConnection_Local.Open();
                SqlCommand l_SqlCommand_Local = new SqlCommand("RptUI_Candidate_Result_Local", l_SqlConnection_Local);
                l_SqlCommand_Local.Parameters.AddWithValue("@mRegionID", "Lahore");
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
                l_SqlCommand_Executive.Parameters.AddWithValue("@mRegionID", "Lahore");
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
            return View("Dashboard");
        }
    }

    //OK
    [HttpGet]
    public ActionResult CandidateVoteInfoIslamabad()
    {
        try
        {
            ModElectionCommissioner? l_LoggedInElectionCommissioner = HttpContext.Session.Get<ModElectionCommissioner>("LoggedInElectionCommissioner");
            if (l_LoggedInElectionCommissioner == null)
            {
                return View("Dashboard");
            }


            ModCanidateResultDataSet l_ModCanidateResultDataSet = new ModCanidateResultDataSet();
            CmConnectionHelper l_CmConnectionHelper = new CmConnectionHelper();
            string l_ConnectionString = l_CmConnectionHelper.Fnc_GetConnectionString();

            using (SqlConnection l_SqlConnection_Local = new SqlConnection(l_ConnectionString))
            {
                l_SqlConnection_Local.Open();
                SqlCommand l_SqlCommand_Local = new SqlCommand("RptUI_Candidate_Result_Local", l_SqlConnection_Local);
                l_SqlCommand_Local.Parameters.AddWithValue("@mRegionID", "Islamabad");
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
                l_SqlCommand_Executive.Parameters.AddWithValue("@mRegionID", "Islamabad");
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
            return View("Dashboard");
        }
    }

    //OK
    [HttpGet]
    public ActionResult CandidateVoteInfoKarachi()
    {
        try
        {
            ModElectionCommissioner? l_LoggedInElectionCommissioner = HttpContext.Session.Get<ModElectionCommissioner>("LoggedInElectionCommissioner");
            if (l_LoggedInElectionCommissioner == null)
            {
                return View("Dashboard");
            }


            ModCanidateResultDataSet l_ModCanidateResultDataSet = new ModCanidateResultDataSet();
            CmConnectionHelper l_CmConnectionHelper = new CmConnectionHelper();
            string l_ConnectionString = l_CmConnectionHelper.Fnc_GetConnectionString();

            using (SqlConnection l_SqlConnection_Local = new SqlConnection(l_ConnectionString))
            {
                l_SqlConnection_Local.Open();
                SqlCommand l_SqlCommand_Local = new SqlCommand("RptUI_Candidate_Result_Local", l_SqlConnection_Local);
                l_SqlCommand_Local.Parameters.AddWithValue("@mRegionID", "Karachi");
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
                l_SqlCommand_Executive.Parameters.AddWithValue("@mRegionID", "Karachi");
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
            return View("Dashboard");
        }
    }

    //OK
    [HttpGet]
    public ActionResult CandidateVoteInfoFaislabad()
    {
        try
        {
            ModElectionCommissioner? l_LoggedInElectionCommissioner = HttpContext.Session.Get<ModElectionCommissioner>("LoggedInElectionCommissioner");
            if (l_LoggedInElectionCommissioner == null)
            {
                return View("Dashboard");
            }


            ModCanidateResultDataSet l_ModCanidateResultDataSet = new ModCanidateResultDataSet();
            CmConnectionHelper l_CmConnectionHelper = new CmConnectionHelper();
            string l_ConnectionString = l_CmConnectionHelper.Fnc_GetConnectionString();

            using (SqlConnection l_SqlConnection_Local = new SqlConnection(l_ConnectionString))
            {
                l_SqlConnection_Local.Open();
                SqlCommand l_SqlCommand_Local = new SqlCommand("RptUI_Candidate_Result_Local", l_SqlConnection_Local);
                l_SqlCommand_Local.Parameters.AddWithValue("@mRegionID", "Faislabad");
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
                l_SqlCommand_Executive.Parameters.AddWithValue("@mRegionID", "Faislabad");
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
            return View("Dashboard");
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
            return View("Dashboard");
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
            return View("Dashboard");
        }
    }
}