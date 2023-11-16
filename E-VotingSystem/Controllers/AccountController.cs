using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
public class AccountController : Controller
{
    [HttpGet]
    public ActionResult Index()
    {
        return View();
    }

    //Pending
    [HttpPost]
    public ActionResult Login(ModMember l_ModMember)
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
                l_SqlCommand_Member.Parameters.AddWithValue("@mMemberID", l_ModMember.MemberID?.Trim());
                l_SqlCommand_Member.Parameters.AddWithValue("@mPassword", l_ModMember.Password?.Trim());
                l_SqlCommand_Member.CommandText = " SELECT * FROM Vw_TBU_Member WHERE MemberID = @mMemberID AND Password = @mPassword ";

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
                    l_ModLoggedInMember.IsEnabled = (bool)l_SqlDataReader_Member["IsEnabled"];
                }
                else
                {
                    return View("ErrorLogin");
                }
                
                if (l_ModLoggedInMember.IsEnabled == false)
                {
                    return View("ErrorEnabled");
                }

                if (string.IsNullOrWhiteSpace(l_ModLoggedInMember.Mobile))
                {
                    return View("ErrorMobile");
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
                        return View("Index");
                    }

                    int l_TotalVoteCount = l_ModLoggedInMember.LcCouncilSeats + l_ModLoggedInMember.ExCouncilSeats;
                    int l_VoteCount = (int)l_Result;

                    if (l_VoteCount >= l_TotalVoteCount)
                    {
                        return View("ErrorVoteLimit");
                    }

                    HttpContext.Session.Set<ModMember>("LoggedInMember", l_ModLoggedInMember);
                    //return RedirectToAction("Index", "OTP", l_ModLoggedInMember);
                    return RedirectToAction("Index", "Profile");
                }
            }
        }
        catch (Exception ex)
        {
            new CmConnectionHelper().Vd_WriteToFile(ex.Message);
            TempData["ErrorMessage"] = ex.Message;
            return View("Index");
        }
    }

    //OK
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
}