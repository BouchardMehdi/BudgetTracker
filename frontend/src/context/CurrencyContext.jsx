import { createContext, useContext, useMemo, useState } from 'react';

const CurrencyContext = createContext(null);
const currencyKey = 'budgettracker_currency';

export const currencyOptions = [
  { value: 'EUR', label: 'EUR' },
  { value: 'USD', label: 'USD' },
  { value: 'GBP', label: 'GBP' },
  { value: 'CAD', label: 'CAD' },
  { value: 'CHF', label: 'CHF' },
];

export function CurrencyProvider({ children }) {
  const [currency, setCurrencyState] = useState(() => localStorage.getItem(currencyKey) || 'EUR');

  function setCurrency(value) {
    localStorage.setItem(currencyKey, value);
    setCurrencyState(value);
  }

  const formatter = useMemo(
    () =>
      new Intl.NumberFormat('fr-FR', {
        style: 'currency',
        currency,
      }),
    [currency]
  );

  const value = useMemo(
    () => ({
      currency,
      setCurrency,
      formatCurrency: (amount) => formatter.format(amount || 0),
    }),
    [currency, formatter]
  );

  return <CurrencyContext.Provider value={value}>{children}</CurrencyContext.Provider>;
}

export function useCurrency() {
  const context = useContext(CurrencyContext);
  if (!context) {
    throw new Error('useCurrency must be used inside CurrencyProvider.');
  }

  return context;
}
