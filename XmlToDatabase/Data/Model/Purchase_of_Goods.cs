// Модель Покупка Товаров
public class Purchase_of_Goods
{
    public int ID_Покупки { get; set; }        // Идентификатор покупки
    public int ID_Товара { get; set; }         // Идентификатор товара
    public int Количество { get; set; }        // Количество товара
    public decimal Общая_стоимость { get; set; } // Общая стоимость покупки
}