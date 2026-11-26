import type { NotaFiscal } from "@/types/nota-fiscal";

const API_URL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5080";

export async function listarNotas(): Promise<NotaFiscal[]> {
  const res = await fetch(`${API_URL}/api/notas`, { cache: "no-store" });
  if (!res.ok) throw new Error(`Erro ${res.status} ao listar notas`);
  return res.json();
}

export async function enviarXml(arquivo: File): Promise<NotaFiscal> {
  const form = new FormData();
  form.append("arquivo", arquivo); // mesmo nome do parâmetro no NotasController
  const res = await fetch(`${API_URL}/api/notas`, { method: "POST", body: form });
  if (!res.ok) throw new Error(await res.text());
  return res.json();
}
