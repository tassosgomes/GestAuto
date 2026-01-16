import axios from 'axios';

export interface ProblemDetails {
  title?: string;
  detail?: string;
  status?: number;
  type?: string;
  instance?: string;
}

export class ProblemDetailsError extends Error {
  title?: string;
  detail?: string;
  status?: number;
  type?: string;
  instance?: string;

  constructor(problem: ProblemDetails, fallbackMessage = 'Erro ao processar requisição') {
    super(problem.detail || problem.title || fallbackMessage);
    this.name = 'ProblemDetailsError';
    this.title = problem.title;
    this.detail = problem.detail;
    this.status = problem.status;
    this.type = problem.type;
    this.instance = problem.instance;
  }
}

export const extractProblemDetails = (error: unknown): ProblemDetails | null => {
  if (!axios.isAxiosError(error)) {
    return null;
  }

  const data = error.response?.data;
  if (!data || typeof data !== 'object') {
    return null;
  }

  const payload = data as Record<string, unknown>;
  const title = typeof payload.title === 'string' ? payload.title : undefined;
  const detail = typeof payload.detail === 'string' ? payload.detail : undefined;
  const status = typeof payload.status === 'number' ? payload.status : undefined;
  const type = typeof payload.type === 'string' ? payload.type : undefined;
  const instance = typeof payload.instance === 'string' ? payload.instance : undefined;

  if (!title && !detail && status === undefined) {
    return null;
  }

  return {
    title,
    detail,
    status,
    type,
    instance,
  };
};

export const handleProblemDetailsError = (error: unknown, fallbackMessage?: string): never => {
  const problem = extractProblemDetails(error);
  if (problem) {
    throw new ProblemDetailsError(problem, fallbackMessage);
  }

  throw error;
};
