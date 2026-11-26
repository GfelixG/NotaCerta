import type { NotaFiscal } from "@/types/nota-fiscal";

export function StatusBadge({ status }: { status: NotaFiscal["status"] }) {
  const cor = status === "Aceita" ? "bg-green-100 text-green-700" : "bg-red-100 text-red-700";
  return <span className={`rounded px-2 py-0.5 text-xs font-medium ${cor}`}>{status}</span>;
}
