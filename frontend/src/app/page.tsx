import { Suspense } from "react";
import { StatusBadge } from "@/components/status-badge";
import { UploadXml } from "@/components/upload-xml";
import { listarNotas } from "@/lib/api";

export default function Home() {
  return (
    <section className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold">NotaCerta</h1>
        <p className="text-zinc-600">Portal de fornecedores: envie o XML da NF-e e veja se ela foi aceita.</p>
      </div>

      <UploadXml />

      {/* Suspense: mostra "Carregando…" enquanto a lista vem da API */}
      <Suspense fallback={<p className="text-zinc-500">Carregando…</p>}>
        <TabelaNotas />
      </Suspense>
    </section>
  );
}

async function TabelaNotas() {
  const notas = await listarNotas().catch(() => null);

  if (notas === null) return <p className="text-red-700">Não foi possível falar com a API. Ela está rodando?</p>;
  if (notas.length === 0) return <p className="text-zinc-600">Nenhuma nota enviada ainda.</p>;

  return (
    <table className="w-full text-left text-sm">
      <thead className="border-b text-zinc-500">
        <tr>
          <th className="py-2">Número</th>
          <th>Emitente</th>
          <th>Valor</th>
          <th>Status</th>
          <th>Motivo</th>
        </tr>
      </thead>
      <tbody>
        {notas.map((n) => (
          <tr key={n.id} className="border-b">
            <td className="py-2">{n.numero}</td>
            <td>{n.razaoSocialEmitente}</td>
            <td>{n.valorTotal.toLocaleString("pt-BR", { style: "currency", currency: "BRL" })}</td>
            <td><StatusBadge status={n.status} /></td>
            <td className="text-zinc-600">{n.motivoRejeicao}</td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}
