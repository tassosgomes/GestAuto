import { Badge } from '@/components/ui/badge';
import { Diamond, Award, Medal } from 'lucide-react';
import { cn } from '@/lib/utils';

interface LeadScoreBadgeProps {
  score: string | undefined;
  showSla?: boolean;
  className?: string;
}

const scoreConfig = {
  Diamond: {
    label: 'Diamante',
    icon: Diamond,
    colorClasses: 'bg-purple-100 text-purple-800 border-purple-300 dark:bg-purple-900 dark:text-purple-100 dark:border-purple-700',
    slaText: 'Atender em 10 min',
    slaColorClasses: 'text-purple-700 dark:text-purple-300',
  },
  Gold: {
    label: 'Ouro',
    icon: Award,
    colorClasses: 'bg-yellow-100 text-yellow-800 border-yellow-300 dark:bg-yellow-900 dark:text-yellow-100 dark:border-yellow-700',
    slaText: 'Atender em 30 min',
    slaColorClasses: 'text-yellow-700 dark:text-yellow-300',
  },
  Silver: {
    label: 'Prata',
    icon: Medal,
    colorClasses: 'bg-gray-100 text-gray-800 border-gray-300 dark:bg-gray-700 dark:text-gray-100 dark:border-gray-600',
    slaText: 'Atender em 2h',
    slaColorClasses: 'text-gray-700 dark:text-gray-300',
  },
  Bronze: {
    label: 'Bronze',
    icon: Medal,
    colorClasses: 'bg-orange-100 text-orange-800 border-orange-300 dark:bg-orange-900 dark:text-orange-100 dark:border-orange-700',
    slaText: 'Baixa Prioridade',
    slaColorClasses: 'text-orange-700 dark:text-orange-300',
  },
};

export function LeadScoreBadge({ score, showSla = false, className }: LeadScoreBadgeProps) {
  if (!score || !(score in scoreConfig)) {
    return null;
  }

  const config = scoreConfig[score as keyof typeof scoreConfig];
  const Icon = config.icon;

  return (
    <div className={cn('flex items-center gap-2', className)}>
      <Badge className={cn('flex items-center gap-1.5 px-3 py-1', config.colorClasses)}>
        <Icon className="h-4 w-4" />
        <span className="font-semibold">{config.label}</span>
      </Badge>
      {showSla && (
        <span className={cn('text-xs font-medium', config.slaColorClasses)}>
          {config.slaText}
        </span>
      )}
    </div>
  );
}
