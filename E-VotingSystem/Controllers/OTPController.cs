using Microsoft.AspNetCore.Mvc;
public class OTPController : Controller
{
    public ActionResult Index(ModMember l_ModMember)
    {
        return View(l_ModMember);
    }

    public ActionResult IndexOfficial(ModElectionOfficial l_ModElectionOfficial)
    {
        return View(l_ModElectionOfficial);
    }

    public ActionResult IndexCommissioner(ModElectionCommissioner l_ModElectionCommissioner)
    {
        return View(l_ModElectionCommissioner);
    }
}