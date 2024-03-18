using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using MomoSwitchPortal.Models.ViewModels.Transaction;
using MomoSwitchPortal.Models.Internals;



namespace MomoSwitchPortal.Models.ViewModels.User
{
    public class UserListViewModel
    {
        public ResponseHeader ResponseHeader { get; set; }
        public List<UserDetailsViewModel> UserList { get; set; }
        public UserFilterRequest UserFilterRequest { get; set; }
        public PaginationMetaData PaginationMetaData { get; set; }



    }
}
