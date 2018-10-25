using DynamicForms.ViewModel;
using System.Threading;
using System.Threading.Tasks;

namespace DynamicForms.ModelBinding
{
    public interface IDynamicFormsService
    {
        Task<DynamicForm> CreateFormModelFromDbAsync(string formUrlSlug, CancellationToken cancellationToken = default(CancellationToken));
        Task PopulateFormModelFromDbAsync(DynamicForm formModel, string formSubmissionId, CancellationToken cancellationToken = default(CancellationToken));
    }
}
