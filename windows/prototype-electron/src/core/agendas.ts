// Agendas enregistrés : enregistrer, ouvrir, dupliquer, modèles (AGD-02).
import type { Agenda } from './types';
import type { JsonStore } from './storage';
import { newId } from './text';

export interface AgendaSummary {
  id: string;
  name: string;
  isTemplate: boolean;
  updatedAt: string;
  count: number;
}

const rel = (id: string) => `agendas/${id}.json`;

export function saveAgenda(store: JsonStore, agenda: Agenda): void {
  agenda.updatedAt = new Date().toISOString();
  store.write(rel(agenda.id), agenda);
}

export function loadAgenda(store: JsonStore, id: string): Agenda | null {
  return store.read<Agenda | null>(rel(id), null);
}

export function listAgendas(store: JsonStore): AgendaSummary[] {
  return store
    .list('agendas')
    .map((f) => store.read<Agenda | null>(`agendas/${f}`, null))
    .filter((a): a is Agenda => !!a)
    .map((a) => ({ id: a.id, name: a.name, isTemplate: !!a.isTemplate, updatedAt: a.updatedAt, count: a.items.length }))
    .sort((a, b) => b.updatedAt.localeCompare(a.updatedAt));
}

/** Copie un agenda (ou un modèle) sous un nouvel identifiant ; la copie n'est jamais un modèle. */
export function duplicateAgenda(source: Agenda, name: string): Agenda {
  const copie: Agenda = JSON.parse(JSON.stringify(source));
  copie.id = newId('agenda');
  copie.name = name;
  copie.isTemplate = false;
  copie.items = copie.items.map((i) => ({ ...i, id: newId('item') }));
  copie.updatedAt = new Date().toISOString();
  return copie;
}
