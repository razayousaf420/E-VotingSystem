using System.Data.SqlClient;
using E_VotingSystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace E_VotingSystem.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Verify(ModUser l_ModUser)
        {
            try
            {
                CmConnectionHelper l_CmConnectionHelper = new CmConnectionHelper();
                string l_ConnectionString = l_CmConnectionHelper.FncGetConnectionString();

                using (SqlConnection l_SqlConnection = new SqlConnection(l_ConnectionString))
                {
                    l_SqlConnection.Open();
                    SqlCommand l_SqlCommand = new SqlCommand();
                    l_SqlCommand.Connection = l_SqlConnection;
                    l_SqlCommand.Parameters.AddWithValue("@MembershipID", l_ModUser.MembershipID);
                    l_SqlCommand.Parameters.AddWithValue("@Password", l_ModUser.Password);
                    l_SqlCommand.CommandText = " SELECT * FROM Vw_TBU_Members WHERE MembershipID = @MembershipID AND Password = @Password ";

                    ModUser l_ModLoggedInUser = new ModUser();
                    SqlDataReader l_SqlDataReader = l_SqlCommand.ExecuteReader();
                    if (l_SqlDataReader.Read())
                    {
                        l_ModLoggedInUser.ImageLocation = l_SqlDataReader["ImageLocation"] as string;
                        l_ModLoggedInUser.ExRegionSeats = l_SqlDataReader["ExCouncilSeats"] as string;
                        l_ModLoggedInUser.LcRegionSeats = l_SqlDataReader["LcCouncilSeats"] as string;
                        l_ModLoggedInUser.PKGUID = l_SqlDataReader["PKGUID"] as string;
                        l_ModLoggedInUser.MembershipID = l_SqlDataReader["MembershipID"] as string;
                        l_ModLoggedInUser.MemberName = l_SqlDataReader["MemberName"] as string;
                        l_ModLoggedInUser.Address1 = l_SqlDataReader["Address1"] as string;
                        l_ModLoggedInUser.Address2 = l_SqlDataReader["Address2"] as string;
                        l_ModLoggedInUser.Email = l_SqlDataReader["Email"] as string;
                        l_ModLoggedInUser.FounderMembers = l_SqlDataReader["FounderMembers"] as string;
                        l_ModLoggedInUser.LifeAndAnnualMembers = l_SqlDataReader["LifeAndAnnualMembers"] as string;
                        l_ModLoggedInUser.CompanysName = l_SqlDataReader["CompanysName"] as string;
                        l_ModLoggedInUser.Region = l_SqlDataReader["Region"] as string;
                        l_ModLoggedInUser.Password = l_ModUser.Password;
                        l_ModLoggedInUser.Mobile = l_SqlDataReader["Mobile"] as string;
                        l_ModLoggedInUser.ContactNo = l_SqlDataReader["ContactNo"] as int?;
                    }

                    if (string.IsNullOrWhiteSpace(l_ModLoggedInUser.Mobile))
                    {
                        return View("ErrorMobile");
                    }

                    if (string.IsNullOrWhiteSpace(l_ModLoggedInUser.PKGUID))
                    {
                        return View("ErrorMobile");
                    }
                    //a

                    DalInsertVoting l_DalInsertVoting = new DalInsertVoting();
                    int lUserCount = l_DalInsertVoting.FncGetRecordCountForUser(l_ModLoggedInUser.PKGUID, l_ConnectionString);

                    int l_ExSeatsCount = int.Parse(l_ModLoggedInUser.ExRegionSeats ?? "0");
                    int l_LcSeatsCount = int.Parse(l_ModLoggedInUser.LcRegionSeats ?? "0");
                    int TotalVotes = l_ExSeatsCount + l_LcSeatsCount;

                    if (lUserCount > TotalVotes)
                    {
                        TempData["ErrorMessage"] = l_ModLoggedInUser.MemberName;
                        return View("Index");
                    }

                    HttpContext.Session.Set<ModUser>("LoggedinUser", l_ModLoggedInUser);
                    return RedirectToAction("Index", "Profile");
                }
            }
            catch (Exception ex)
            {
                new CmConnectionHelper().WriteToFile(ex.Message);
                TempData["ErrorMessage"] = ex.Message;
                return View("Index");
            }
        }
    }
}