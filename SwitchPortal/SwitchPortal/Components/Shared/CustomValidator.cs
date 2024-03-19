using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace SwitchPortal.Components.Shared
{
    public class CustomValidator : ComponentBase
    {
        private ValidationMessageStore _messageStore;
        [CascadingParameter]
        public EditContext editContext { get; set; }

        protected override void OnInitialized()
        {
            if (editContext == null)
            {
                return;
            }

            _messageStore = new ValidationMessageStore(editContext);
            editContext.OnValidationRequested += (s, e) => _messageStore.Clear();
            editContext.OnFieldChanged += (s, e) => _messageStore.Clear(e.FieldIdentifier);
        }

        public void DisplayErrors(Dictionary<string, List<string>> errors)
        {
            foreach (var error in errors)
            {
                _messageStore.Add(editContext.Field(error.Key), error.Value);
            }

            editContext.NotifyValidationStateChanged();
        }


        public void ClearErrors()
        {
            _messageStore.Clear();
        }
    }
}
