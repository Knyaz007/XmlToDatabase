// Метод для загрузки из XML файла
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;


namespace XmlToDatabase
{
    public class OrderLoader
    {
        public static IEnumerable<Order> LoadOrdersFromXml(string filePath)
        {
            var document = XDocument.Load(filePath);
            var orders = from orderElement in document.Descendants("order")
                         select new Order
                         {
                             Id = (int)orderElement.Element("no"),
                             Date = DateTime.ParseExact((string)orderElement.Element("reg_date"), "yyyy.MM.dd", null),
                             Sum = (decimal)orderElement.Element("sum"),
                             User = new User
                             {
                                 FullName = (string)orderElement.Element("user").Element("fio"),
                                 Email = (string)orderElement.Element("user").Element("email")
                             },
                             Products = orderElement.Elements("product").Select(product => new Product
                             {
                                 Name = (string)product.Element("name"),
                                 Quantity = (int)product.Element("quantity"),
                                 Price = (decimal)product.Element("price")
                             }).ToList()
                         };

            return orders;
        }
    }

}
