// Модель заказа
using XmlToDatabase;

public class Order
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public decimal Sum { get; set; }
    public User User { get; set; }
    public List<Product> Products { get; set; }
}