import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert';
import { Diamond, Award, Medal, TrendingUp } from 'lucide-react';
import { cn } from '@/lib/utils';

interface LeadActionFeedbackProps {
  score: string | undefined;
  className?: string;
}

const feedbackConfig = {
  Diamond: {
    title: 'Prioridade Máxima',
    description: 'Lead de alto valor. Recomendado acompanhamento gerencial e atendimento imediato.',
    icon: Diamond,
    colorClasses: 'border-purple-300 bg-purple-50 text-purple-900 dark:border-purple-700 dark:bg-purple-950 dark:text-purple-100',
    iconColorClasses: 'text-purple-600 dark:text-purple-400',
  },
  Gold: {
    title: 'Alta Prioridade',
    description: 'Excelente oportunidade. Priorize o contato e prepare uma proposta competitiva.',
    icon: Award,
    colorClasses: 'border-yellow-300 bg-yellow-50 text-yellow-900 dark:border-yellow-700 dark:bg-yellow-950 dark:text-yellow-100',
    iconColorClasses: 'text-yellow-600 dark:text-yellow-400',
  },
  Silver: {
    title: 'Média Prioridade',
    description: 'Boa oportunidade. Foque em oferecer opções de financiamento parcial e demonstre os benefícios.',
    icon: Medal,
    colorClasses: 'border-gray-300 bg-gray-50 text-gray-900 dark:border-gray-600 dark:bg-gray-900 dark:text-gray-100',
    iconColorClasses: 'text-gray-600 dark:text-gray-400',
  },
  Bronze: {
    title: 'Baixa Prioridade',
    description: 'Lead de nutrição. Continue o relacionamento através de e-mails e conteúdos automáticos.',
    icon: TrendingUp,
    colorClasses: 'border-orange-300 bg-orange-50 text-orange-900 dark:border-orange-700 dark:bg-orange-950 dark:text-orange-100',
    iconColorClasses: 'text-orange-600 dark:text-orange-400',
  },
};

export function LeadActionFeedback({ score, className }: LeadActionFeedbackProps) {
  if (!score || !(score in feedbackConfig)) {
    return null;
  }

  const config = feedbackConfig[score as keyof typeof feedbackConfig];
  const Icon = config.icon;

  return (
    <Alert className={cn(config.colorClasses, className)}>
      <Icon className={cn('h-5 w-5', config.iconColorClasses)} />
      <AlertTitle className="font-semibold">{config.title}</AlertTitle>
      <AlertDescription className="mt-1">{config.description}</AlertDescription>
    </Alert>
  );
}
