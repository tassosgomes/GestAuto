import React, { forwardRef } from 'react';
import type { InputHTMLAttributes } from 'react';
import { Input } from '@/components/ui/input';

interface CurrencyInputProps extends Omit<InputHTMLAttributes<HTMLInputElement>, 'onChange' | 'value'> {
  value?: number | string;
  onChange?: (value: number) => void;
}

export const CurrencyInput = forwardRef<HTMLInputElement, CurrencyInputProps>(
  ({ value, onChange, ...props }, ref) => {
    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
      const rawValue = e.target.value;
      const digits = rawValue.replace(/\D/g, '');
      
      if (!digits) {
        onChange?.(0);
        return;
      }
      
      const numericValue = parseInt(digits, 10) / 100;
      onChange?.(numericValue);
    };

    const displayValue = React.useMemo(() => {
      if (value === undefined || value === null || value === '') return '';
      
      const numValue = typeof value === 'string' ? parseFloat(value) : value;
      if (isNaN(numValue)) return '';
      
      return new Intl.NumberFormat('pt-BR', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2,
      }).format(numValue);
    }, [value]);

    return (
      <div className="relative">
        <span className="absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground text-sm">
          R$
        </span>
        <Input
          {...props}
          ref={ref}
          type="text"
          inputMode="numeric"
          value={displayValue}
          onChange={handleChange}
          className="pl-10"
        />
      </div>
    );
  }
);

CurrencyInput.displayName = 'CurrencyInput';
