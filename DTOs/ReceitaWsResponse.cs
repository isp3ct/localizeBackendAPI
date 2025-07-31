using Newtonsoft.Json;

public class ReceitaWsResponse
{
    [JsonProperty("nome")]
    public string Nome { get; set; }

    [JsonProperty("fantasia")]
    public string Fantasia { get; set; }

    [JsonProperty("cnpj")]
    public string Cnpj { get; set; }

    [JsonProperty("situacao")]
    public string Situacao { get; set; }

    [JsonProperty("abertura")]
    public string Abertura { get; set; }

    [JsonProperty("tipo")]
    public string Tipo { get; set; }

    [JsonProperty("natureza_juridica")]
    public string NaturezaJuridica { get; set; }

    [JsonProperty("atividade_principal")]
    public List<AtividadeDto> AtividadesPrincipais { get; set; }

    [JsonProperty("logradouro")]
    public string Logradouro { get; set; }

    [JsonProperty("numero")]
    public string Numero { get; set; }

    [JsonProperty("complemento")]
    public string Complemento { get; set; }

    [JsonProperty("bairro")]
    public string Bairro { get; set; }

    [JsonProperty("municipio")]
    public string Municipio { get; set; }

    [JsonProperty("uf")]
    public string Uf { get; set; }

    [JsonProperty("cep")]
    public string Cep { get; set; }

    [JsonProperty("status")]
    public string Status { get; set; }

    [JsonProperty("message")]
    public string Message { get; set; }
}

public class AtividadeDto
{
    [JsonProperty("code")]
    public string Code { get; set; }

    [JsonProperty("text")]
    public string Text { get; set; }
}
