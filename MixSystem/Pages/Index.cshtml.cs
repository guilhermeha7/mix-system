using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MixSystem.Models;
using NAudio.Wave;

/* Usando Razor Pages cada view possui seo próprio arquivo de lógica com eventos que podem ser executados */

namespace MixSystem.Pages
{
    public class IndexModel : PageModel
    {
        [BindProperty] public Mix Mix { get; set; } 

        public void OnGet()
        {

        }

        public IActionResult OnPost(Mix mix) //IActionResult retorna uma página
        {
            
            if (!ModelState.IsValid) //Se houver erros no envio do objeto
            {
                return Page(); // Isso recarrega a própria página Index.cshtml onde você está
            }

            try
            {
                List<string> musicas = Directory.GetFiles(Mix.Diretorio, "*.wav").ToList(); //O método Directory.GetFiles retorna um array de strings (string[]), onde cada string guarda o caminho completo de cada arquivo .wav encontrado.

                if (musicas.Count == 0)
                {
                    ViewData["mensagemDeErro"] = "Nenhuma música .wav foi encontrada";
                    return Page();
                }

                //Impede que o usuário queira sortear mais músicas do que tem na pasta
                int contagem = musicas.Count();
                if (contagem < mix.QtdDeMusicas)
                    mix.QtdDeMusicas = contagem;

                var musicasSorteadas = musicas
                    .OrderBy(m => Guid.NewGuid()) // Cada música receberá um Guid, um código alfanumérico gerado aleatoriamente (Por exemplo: 3F2504E0-4F89-41D3-9A0C-0305E82C3301). OrderBy ordena do menor para o maior por padrão
                    .Take(mix.QtdDeMusicas.Value)       // Pega apenas a quantidade que você definiu
                    .ToList();                    // Transforma de volta em uma lista


                string caminhoSaida = Path.Combine(mix.Diretorio, "MixGerado"+Guid.NewGuid()+".wav");


                using (WaveFileWriter writer = new WaveFileWriter(caminhoSaida, new WaveFormat(48000, 16, 2))) // Padrão CD: 44.1kHz, 16 bits, Stereo
                {
                    foreach (string caminhoMusica in musicasSorteadas)
                    {
                        using (WaveFileReader reader = new WaveFileReader(caminhoMusica))
                        {
                            IWaveProvider streamParaGravar = reader;

                            // Se for diferente de 48k, a gente "envelopa" o reader no resampler
                            if (reader.WaveFormat.SampleRate != 48000)
                            {
                                var outFormat = new WaveFormat(48000, 16, 2);
                                streamParaGravar = new MediaFoundationResampler(reader, outFormat)
                                {
                                    ResamplerQuality = 60
                                };
                            }

                            // Agora o loop é um só, não importa se houve conversão ou não
                            byte[] buffer = new byte[1024];
                            int read;
                            while ((read = streamParaGravar.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                writer.Write(buffer, 0, read);
                            }

                            // Se criou um resampler, ele precisa ser descartado para liberar memória
                            (streamParaGravar as IDisposable)?.Dispose();
                        }
                    }
                }
                ViewData["mensagemDeSucesso"] = "O mix foi gerado com sucesso!";
            }
            catch (Exception ex)
            {
                ViewData["mensagemDeErro"] = "Ocorreu um erro ao gerar o mix!";
            }

            return Page();
        }
    }
}
