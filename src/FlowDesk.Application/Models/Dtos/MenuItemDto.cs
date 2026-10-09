namespace FlowDesk.Application.Models.Dtos;

public class MenuItemDto
{
    // Chiave di traduzione del frontend, es. "menusidebar.label.customers".
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Path { get; set; }
    public List<MenuItemDto> Children { get; set; } = [];
}
