import { useEffect, useState } from 'react';
import { statsApi } from '../api/statsApi';
import StatCard from '../components/StatCard';
import { getApiErrorMessage } from '../utils/apiError';

const currencyFormatter = new Intl.NumberFormat('fr-FR', {
  style: 'currency',
  currency: 'EUR',
});

export default function Dashboard() {
  const [summary, setSummary] = useState({ totalIncome: 0, totalExpense: 0, balance: 0 });
  const [byCategory, setByCategory] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    async function loadStats() {
      try {
        const [summaryData, categoryData] = await Promise.all([
          statsApi.getSummary(),
          statsApi.getByCategory(),
        ]);
        setSummary(summaryData);
        setByCategory(categoryData);
      } catch (loadError) {
        setError(getApiErrorMessage(loadError));
      } finally {
        setIsLoading(false);
      }
    }

    loadStats();
  }, []);

  return (
    <section className="page-stack">
      <div className="page-header">
        <div>
          <p className="eyebrow">Vue generale</p>
          <h1>Dashboard</h1>
        </div>
      </div>

      <div className="stats-grid">
        <StatCard title="Revenus" value={currencyFormatter.format(summary.totalIncome)} tone="income" />
        <StatCard title="Depenses" value={currencyFormatter.format(summary.totalExpense)} tone="expense" />
        <StatCard title="Solde" value={currencyFormatter.format(summary.balance)} tone={summary.balance >= 0 ? 'income' : 'expense'} />
      </div>

      {error && <div className="alert error">{error}</div>}

      <section className="section-block">
        <div className="section-title">
          <h2>Par categorie</h2>
        </div>
        {isLoading ? (
          <div className="empty-state">Chargement...</div>
        ) : byCategory.length === 0 ? (
          <div className="empty-state">Aucune statistique.</div>
        ) : (
          <div className="category-stats">
            {byCategory.map((item) => (
              <div className="category-stat-row" key={`${item.categoryId}-${item.type}`}>
                <div>
                  <strong>{item.categoryName}</strong>
                  <span>{item.type === 'income' ? 'Revenu' : 'Depense'}</span>
                </div>
                <strong className={`amount ${item.type}`}>{currencyFormatter.format(item.total)}</strong>
              </div>
            ))}
          </div>
        )}
      </section>
    </section>
  );
}
