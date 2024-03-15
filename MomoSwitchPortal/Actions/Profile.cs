using Microsoft.EntityFrameworkCore;
using Momo.Common.Actions;
using MomoSwitchPortal.Models;
using MomoSwitchPortal.Models.Database;
using MomoSwitchPortal.Models.ViewModels.Profile;

namespace MomoSwitchPortal.Actions
{
    public interface IProfile
    {
        Task<ProfileDetailsViewModel> GetProfile(int id);        
    }
    public class Profile : IProfile
    {
        private readonly ILog Log;

        public Profile(ILog log)
        {
            Log = log;
        }

        public async Task<ProfileDetailsViewModel> GetProfile(int id)
        {
            try
            {
                var db = new MomoSwitchDbContext();
                var userInDatabase = await db.PortalUserTb.SingleOrDefaultAsync(x => x.Id == id);

                if (userInDatabase == null)
                {
                    Log.Write("Profile:GetProfile", $"eRR: User doesn't exist");
                    return new ProfileDetailsViewModel
                    {
                        ResponseHeader = new ResponseHeader 
                        {
                            ResponseCode = "01",
                            ResponseMessage = "Something went wrong. Please try again"
                        }
                        
                    };

                }


                return new ProfileDetailsViewModel
                {
                    ResponseHeader = new ResponseHeader
                    {
                        ResponseCode = "00",
                        ResponseMessage = "Success"
                    },
                    ProfileDetails = new ProfileDetail
                    {
                        FullName = $"{userInDatabase.LastName} {userInDatabase.FirstName}",
                        Joined = userInDatabase.EntryDate.ToString("dddd, dd MMMM yyyy"),
                        Role = userInDatabase.Role,
                        Username = userInDatabase.Username
                    }
                };
            }
            catch (Exception ex)
            {
                Log.Write("Profile:GetProfile", $"eRR: {ex.Message}");
                return new ProfileDetailsViewModel
                {
                    ResponseHeader = new ResponseHeader
                    {
                        ResponseCode = "01",
                        ResponseMessage = "Something went wrong."
                    }

                };
            }
        }
    }
}
