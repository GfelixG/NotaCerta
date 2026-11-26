"use client";

import { useRef, useState, type FormEvent } from "react";
import { useRouter } from "next/navigation";
import { enviarXml } from "@/lib/api";

type Resultado = { tipo: "sucesso" | "aviso" | "erro"; texto: string };

const estilos: Record<Resultado["tipo"], string> = {
  sucesso: "border-green-300 bg-green-50 text-green-800",
  aviso: "border-amber-300 bg-amber-50 text-amber-900",
  erro: "border-red-300 bg-red-50 text-red-800",
};

export function UploadXml() {
  const router = useRouter();
  const input = useRef<HTMLInputElement>(null);
  const [arquivo, setArquivo] = useState<File | null>(null);
  const [enviando, setEnviando] = useState(false);
  const [resultado, setResultado] = useState<Resultado | null>(null);

  async function enviar(e: FormEvent) {
    e.preventDefault();
    if (!arquivo) return;

    setEnviando(true);
    setResultado(null);
    try {
      const nota = await enviarXml(arquivo);
      setResultado(
        nota.status === "Aceita"
          ? { tipo: "sucesso", texto: `Nota ${nota.numero} aceita.` }
          : { tipo: "aviso", texto: `Nota rejeitada: ${nota.motivoRejeicao}` },
      );
      setArquivo(null);
      if (input.current) input.current.value = "";
      router.refresh(); // recarrega a tabela com a nota nova
    } catch (erro) {
      // 400 (arquivo ilegível), 409 (duplicada) ou API fora do ar
      const texto = erro instanceof Error ? erro.message : "Erro inesperado.";
      setResultado({
        tipo: "erro",
        texto: texto === "Failed to fetch" ? "Não foi possível falar com a API. Ela está rodando?" : texto,
      });
    } finally {
      setEnviando(false);
    }
  }

  return (
    <form onSubmit={enviar} className="space-y-3 rounded border border-zinc-200 p-4">
      <label className="block text-sm font-medium" htmlFor="arquivo-xml">
        XML da NF-e
      </label>
      <div className="flex flex-wrap items-center gap-3">
        <input
          ref={input}
          id="arquivo-xml"
          type="file"
          accept=".xml,text/xml,application/xml"
          onChange={(e) => setArquivo(e.target.files?.[0] ?? null)}
          className="text-sm file:mr-3 file:rounded file:border-0 file:bg-zinc-100 file:px-3 file:py-1.5"
        />
        <button
          type="submit"
          disabled={!arquivo || enviando}
          className="rounded bg-zinc-900 px-4 py-2 text-sm text-white disabled:opacity-40"
        >
          {enviando ? "Enviando…" : "Enviar"}
        </button>
      </div>

      {resultado && (
        <p role="status" className={`rounded border px-3 py-2 text-sm ${estilos[resultado.tipo]}`}>
          {resultado.texto}
        </p>
      )}
    </form>
  );
}
