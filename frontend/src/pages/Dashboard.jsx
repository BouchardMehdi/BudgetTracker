import { useEffect, useState } from 'react';
import { statsApi } from '../api/statsApi';
import StatCard from '../components/StatCard';
import { getApiErrorMessage } from '../utils/apiError';

const currencyFormatter = new Intl.NumberFormat('fr-FR', {
  style: 'currency',
  currency: 'EUR',
});

const dateFormatter = new Intl.DateTimeFormat('fr-FR');

const emptySummary = { totalIncome: 0, totalExpense: 0, balance: 0 };

const periodLabels = {
  'current-month': 'Mois courant',
  'previous-month': 'Mois precedent',
  'current-year': 'Annee en cours',
};

export default function Dashboard() {
  const [summary, setSummary] = useState(emptySummary);
  const [currentMonth, setCurrentMonth] = useState(emptySummary);
  const [previousMonth, setPreviousMonth] = useState(emptySummary);
  const [currentYear, setCurrentYear] = useState(emptySummary);
  const [expenseCategories, setExpenseCategories] = useState([]);
  const [latestTransactions, setLatestTransactions] = useState([]);
  const [selectedPeriod, setSelectedPeriod] = useState('current-month');
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    async function loadStats() {
      setIsLoading(true);
      setError('');

      try {
        const [
          summaryData,
          currentMonthData,
          previousMonthData,
          currentYearData,
          latestData,
        ] = await Promise.all([
          statsApi.getSummary('all'),
          statsApi.getSummary('current-month'),
          statsApi.getSummary('previous-month'),
          statsApi.getSummary('current-year'),
          statsApi.getLatestTransactions(5),
        ]);

        setSummary(summaryData);
        setCurrentMonth(currentMonthData);
        setPreviousMonth(previousMonthData);
        setCurrentYear(currentYearData);
        setLatestTransactions(latestData);
      } catch (loadError) {
        setError(getApiErrorMessage(loadError));
      } finally {
        setIsLoading(false);
      }
    }

    loadStats();
  }, []);

  useEffect(() => {
    async function loadCategoryStats() {
      setError('');

      try {
        const data = await statsApi.getByCategory({ period: selectedPeriod, type: 'expense' });
        setExpenseCategories(data);
      } catch (loadError) {
        setError(getApiErrorMessage(loadError));
      }
    }

    loadCategoryStats();
  }, [selectedPeriod]);

  const periodSummary = {
    'current-month': currentMonth,
    'previous-month': previousMonth,
    'current-year': currentYear,
  }[selectedPeriod];

  const monthComparison = currentMonth.balance - previousMonth.balance;

  return (
    <section className="page-stack">
      <div className="page-header">
        <div>
          <p className="eyebrow">Vue generale</p>
          <h1>Dashboard</h1>
        </div>
      </div>

      <div className="stats-grid">
        <StatCard title="Revenus total" value={currencyFormatter.format(summary.totalIncome)} tone="income" />
        <StatCard title="Depenses total" value={currencyFormatter.format(summary.totalExpense)} tone="expense" />
        <StatCard title="Solde total" value={currencyFormatter.format(summary.balance)} tone={summary.balance >= 0 ? 'income' : 'expense'} />
      </div>

      {error && <div className="alert error">{error}</div>}

      <section className="dashboard-grid">
        <article className="section-block dashboard-panel">
          <div className="section-title dashboard-title">
            <div>
              <h2>Revenus vs depenses</h2>
              <span>{periodLabels[selectedPeriod]}</span>
            </div>
            <select value={selectedPeriod} onChange={(event) => setSelectedPeriod(event.target.value)}>
              <option value="current-month">Mois courant</option>
              <option value="previous-month">Mois precedent</option>
              <option value="current-year">Annee en cours</option>
            </select>
          </div>

          <div className="compare-grid">
            <div className="compare-item income">
              <span>Revenus</span>
              <strong>{currencyFormatter.format(periodSummary.totalIncome)}</strong>
            </div>
            <div className="compare-item expense">
              <span>Depenses</span>
              <strong>{currencyFormatter.format(periodSummary.totalExpense)}</strong>
            </div>
          </div>

          <div className="balance-strip">
            <span>Solde de la periode</span>
            <strong className={`amount ${periodSummary.balance >= 0 ? 'income' : 'expense'}`}>
              {currencyFormatter.format(periodSummary.balance)}
            </strong>
          </div>
        </article>

        <article className="section-block dashboard-panel">
          <div className="section-title">
            <h2>Periodes</h2>
          </div>
          <div className="period-list">
            <PeriodRow label="Mois courant" summary={currentMonth} />
            <PeriodRow label="Mois precedent" summary={previousMonth} />
            <PeriodRow label="Annee en cours" summary={currentYear} />
          </div>
          <div className="balance-strip compact">
            <span>Evolution vs mois precedent</span>
            <strong className={`amount ${monthComparison >= 0 ? 'income' : 'expense'}`}>
              {currencyFormatter.format(monthComparison)}
            </strong>
          </div>
        </article>
      </section>

      <section className="dashboard-grid">
        <article className="section-block dashboard-panel">
          <div className="section-title dashboard-title">
            <div>
              <h2>Depenses par categorie</h2>
              <span>{periodLabels[selectedPeriod]}</span>
            </div>
          </div>
          {isLoading ? (
            <div className="empty-state">Chargement...</div>
          ) : expenseCategories.length === 0 ? (
            <div className="empty-state">Aucune depense sur cette periode.</div>
          ) : (
            <div className="category-stats">
              {expenseCategories.map((item) => (
                <CategoryBar key={item.categoryId} item={item} maxTotal={expenseCategories[0]?.total || 1} />
              ))}
            </div>
          )}
        </article>

        <article className="section-block dashboard-panel">
          <div className="section-title">
            <h2>Dernieres transactions</h2>
          </div>
          {isLoading ? (
            <div className="empty-state">Chargement...</div>
          ) : latestTransactions.length === 0 ? (
            <div className="empty-state">Aucune transaction.</div>
          ) : (
            <div className="latest-list">
              {latestTransactions.map((transaction) => (
                <div className="latest-row" key={transaction.id}>
                  <div>
                    <strong>{transaction.title}</strong>
                    <span>
                      {transaction.categoryName} - {dateFormatter.format(new Date(transaction.transactionDate))}
                    </span>
                  </div>
                  <strong className={`amount ${transaction.type}`}>
                    {currencyFormatter.format(transaction.amount)}
                  </strong>
                </div>
              ))}
            </div>
          )}
        </article>
      </section>
    </section>
  );
}

function PeriodRow({ label, summary }) {
  return (
    <div className="period-row">
      <div>
        <strong>{label}</strong>
        <span>{currencyFormatter.format(summary.totalIncome)} revenus</span>
      </div>
      <strong className={`amount ${summary.balance >= 0 ? 'income' : 'expense'}`}>
        {currencyFormatter.format(summary.balance)}
      </strong>
    </div>
  );
}

function CategoryBar({ item, maxTotal }) {
  const width = Math.max(6, Math.round((item.total / maxTotal) * 100));

  return (
    <div className="category-bar-row">
      <div className="category-bar-heading">
        <strong>{item.categoryName}</strong>
        <span>{currencyFormatter.format(item.total)}</span>
      </div>
      <div className="category-bar-track">
        <span style={{ width: `${width}%` }} />
      </div>
    </div>
  );
}
