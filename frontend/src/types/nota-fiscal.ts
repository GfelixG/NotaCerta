// Mesmo formato que a API devolve em GET /api/notas
export interface NotaFiscal {
  id: string;
  chaveAcesso: string;
  numero: string;
  cnpjEmitente: string;
  razaoSocialEmitente: string;
  valorTotal: number;
  status: "Aceita" | "Rejeitada";
  motivoRejeicao: string | null;
  recebidaEm: string;
}
