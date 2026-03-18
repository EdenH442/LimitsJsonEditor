using LimitsEditor.Models;

namespace LimitsEditor.Services;

public interface IAddTestDialogService
{
    Step? ShowDialog(string sequenceName);
}
