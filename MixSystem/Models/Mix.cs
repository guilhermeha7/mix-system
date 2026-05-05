using System.ComponentModel.DataAnnotations;

namespace MixSystem.Models
{
    public class Mix
    {
        [Required(ErrorMessage = "Campo obrigatório")]
        [Range(1, int.MaxValue, ErrorMessage = "O valor deve ser maior que 0")]
        public int? QtdDeMusicas { get; set; } //int?: indica que a variável aceita receber valor nulo ou inteiro

        [Required(ErrorMessage = "Campo obrigatório")]
        public string Diretorio { get; set; }
    }
}
