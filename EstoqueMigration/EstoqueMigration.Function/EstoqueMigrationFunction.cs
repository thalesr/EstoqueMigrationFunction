using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using EstoqueMigration.Function.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EstoqueMigration.Function
{
    public class EstoqueMigrationFunction
    {
        [FunctionName("EstoqueMigrationFunction")]
        [Timeout("02:00:00")]
        public async Task Run([TimerTrigger("0 */1 * * * *")] TimerInfo myTimer, ILogger log)
        {

            try
            {
                List<Produto> produtos = null;
                var client = new HttpClient();

                /*
                 * 
                 * RECUPERA A LISTA DE ITENS QUE SERÃO MIGRADOS
                 * 
                 */

                var getRequestUri = "https://thales-az-test.free.beeceptor.com/ProdutosDoDia";
                using (var response = await client.GetAsync(getRequestUri))
                {
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();
                    produtos = JsonConvert.DeserializeObject<List<Produto>>(body);
                }

                /*
                 * 
                 * ENVIA A LISTA DE ITENS PARA A API DE DESTINO
                 * 
                 */

                if (produtos != null)
                {

                    foreach (var produto in produtos)
                    {

                        var postRequestUri = "https://thales-az-test.free.beeceptor.com/Estoque";
                        var json = JsonConvert.SerializeObject(produto);
                        using (var response = await client.PostAsync(postRequestUri, new StringContent(json, Encoding.UTF8, "application/json")))
                        {
                            response.EnsureSuccessStatusCode();
                            if (response.StatusCode == System.Net.HttpStatusCode.Created)
                            {
                                log.LogInformation($"O produto {produto.Nome} foi incluído com sucesso.");
                            }
                        }

                    };

                }
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
            }

        }
    }
}
