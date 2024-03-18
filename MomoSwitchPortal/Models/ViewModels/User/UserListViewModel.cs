using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using static MomoSwitchPortal.Models.ViewModels.Pager.PaginationParams;



namespace MomoSwitchPortal.Models.ViewModels.User
{
    public class UserListViewModel
    {
        public ResponseHeader ResponseHeader { get; set; }
        public X.PagedList.IPagedList<UserDetailsViewModel> UserList { get; set; }
        public string Username { get; set; }


    }
}
