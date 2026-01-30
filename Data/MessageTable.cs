using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Portfolyo.Data
{
    [Table("MessagesTable")] 
    public class MessageTable
    {
        [Key]
        public int MessageId { get; set; }

        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? MessageContent { get; set; }
        public DateTime MessageDate { get; set; }
       

       

    }
}
