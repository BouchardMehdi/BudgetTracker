import { useEffect, useState } from 'react';
import { budgetsApi } from '../api/budgetsApi';
import { categoriesApi } from '../api/categoriesApi';
import { statsApi } from '../api/statsApi';
import StatCard from '../components/StatCard';
import { useCurrency } from '../context/CurrencyContext';
import { getApiErrorMessage } from '../utils/apiError';

const dateFormatter = new Intl.DateTimeFormat('fr-FR');

const emptySummary = { totalIncome: 0, totalExpense: 0, balance: 0 };

const periodLabels = {
  'current-month': 'Mois courant',
  'previous-month': 'Mois precedent',
  'current-year': 'Annee en cours',
};

export default function Dashboard() {
  const { formatCurrency } = useCurrency();
  const [summary, setSummary] = useState(emptySummary);
  const [currentMonth, setCurrentMonth] = useState(emptySummary);
  const [previousMonth, setPreviousMonth] = useState(emptySummary);
  const [currentYear, setCurrentYear] = useState(emptySummary);
  const [expenseCategories, setExpenseCategories] = useState([]);
  const [budgetProgress, setBudgetProgress] = useState([]);
  const [budgets, setBudgets] = useState([]);
  const [expenseCategoryOptions, setExpenseCategoryOptions] = useState([]);
  const [budgetForm, setBudgetForm] = useState({ categoryId: '', amount: '' });
  const [budgetMessage, setBudgetMessage] = useState('');
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
          categoryData,
          budgetData,
        ] = await Promise.all([
          statsApi.getSummary('all'),
          statsApi.getSummary('current-month'),
          statsApi.getSummary('previous-month'),
          statsApi.getSummary('current-year'),
          statsApi.getLatestTransactions(5),
          categoriesApi.getAll(),
          budgetsApi.getAll(),
        ]);

        setSummary(summaryData);
        setCurrentMonth(currentMonthData);
        setPreviousMonth(previousMonthData);
        setCurrentYear(currentYearData);
        setLatestTransactions(latestData);
        setExpenseCategoryOptions(categoryData.filter((category) => category.type === 'expense'));
        setBudgets(budgetData);
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
        const [categoryStatsData, budgetProgressData] = await Promise.all([
          statsApi.getByCategory({ period: selectedPeriod, type: 'expense' }),
          budgetsApi.getProgress(selectedPeriod),
        ]);
        setExpenseCategories(categoryStatsData);
        setBudgetProgress(budgetProgressData);
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

  function updateBudgetForm(event) {
    const { name, value } = event.target;
    setBudgetForm((current) => {
      if (name === 'categoryId') {
        const existingBudget = budgets.find((budget) => budget.categoryId === Number(value));
        return { categoryId: value, amount: existingBudget?.amount?.toString() || '' };
      }

      return { ...current, [name]: value };
    });
  }

  async function saveBudget(event) {
    event.preventDefault();
    setBudgetMessage('');
    setError('');

    if (!budgetForm.categoryId || Number(budgetForm.amount) <= 0) {
      setError('Choisis une categorie et un montant mensuel superieur a 0.');
      return;
    }

    try {
      const existingBudget = budgets.find((budget) => budget.categoryId === Number(budgetForm.categoryId));
      if (existingBudget) {
        await budgetsApi.update(existingBudget.id, { amount: Number(budgetForm.amount) });
      } else {
        await budgetsApi.create({
          categoryId: Number(budgetForm.categoryId),
          amount: Number(budgetForm.amount),
        });
      }

      const [budgetData, progressData] = await Promise.all([
        budgetsApi.getAll(),
        budgetsApi.getProgress(selectedPeriod),
      ]);
      setBudgets(budgetData);
      setBudgetProgress(progressData);
      setBudgetForm({ categoryId: '', amount: '' });
      setBudgetMessage('Budget enregistre.');
    } catch (saveError) {
      setError(getApiErrorMessage(saveError));
    }
  }

  return (
    <section className="page-stack">
      <div className="page-header">
        <div>
          <p className="eyebrow">Vue generale</p>
          <h1>Dashboard</h1>
        </div>
      </div>

      <div className="stats-grid">
        <StatCard title="Revenus total" value={formatCurrency(summary.totalIncome)} tone="income" />
        <StatCard title="Depenses total" value={formatCurrency(summary.totalExpense)} tone="expense" />
        <StatCard title="Solde total" value={formatCurrency(summary.balance)} tone={summary.balance >= 0 ? 'income' : 'expense'} />
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
              <strong>{formatCurrency(periodSummary.totalIncome)}</strong>
            </div>
            <div className="compare-item expense">
              <span>Depenses</span>
              <strong>{formatCurrency(periodSummary.totalExpense)}</strong>
            </div>
          </div>

          <div className="balance-strip">
            <span>Solde de la periode</span>
            <strong className={`amount ${periodSummary.balance >= 0 ? 'income' : 'expense'}`}>
              {formatCurrency(periodSummary.balance)}
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
              {formatCurrency(monthComparison)}
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
                    {formatCurrency(transaction.amount)}
                  </strong>
                </div>
              ))}
            </div>
          )}
        </article>
      </section>

      <section className="section-block dashboard-panel">
        <div className="section-title dashboard-title">
          <div>
            <h2>Budgets mensuels</h2>
            <span>Progression sur {periodLabels[selectedPeriod].toLowerCase()}</span>
          </div>
        </div>

        <form className="budget-form" onSubmit={saveBudget}>
          <label>
            Categorie
            <select name="categoryId" value={budgetForm.categoryId} onChange={updateBudgetForm}>
              <option value="">Selectionner</option>
              {expenseCategoryOptions.map((category) => (
                <option key={category.id} value={category.id}>
                  {category.name}
                </option>
              ))}
            </select>
          </label>
          <label>
            Budget mensuel
            <input
              name="amount"
              type="number"
              min="0.01"
              step="0.01"
              value={budgetForm.amount}
              onChange={updateBudgetForm}
              placeholder="400"
            />
          </label>
          <button className="primary-button" type="submit">
            Enregistrer
          </button>
        </form>

        {budgetMessage && <div className="alert success">{budgetMessage}</div>}

        {budgetProgress.length === 0 ? (
          <div className="empty-state">Aucun budget defini.</div>
        ) : (
          <div className="budget-grid">
            {budgetProgress.map((budget) => (
              <BudgetProgressRow key={budget.budgetId} budget={budget} formatCurrency={formatCurrency} />
            ))}
          </div>
        )}
      </section>
    </section>
  );
}

function PeriodRow({ label, summary }) {
  const { formatCurrency } = useCurrency();

  return (
    <div className="period-row">
      <div>
        <strong>{label}</strong>
        <span>{formatCurrency(summary.totalIncome)} revenus</span>
      </div>
      <strong className={`amount ${summary.balance >= 0 ? 'income' : 'expense'}`}>
        {formatCurrency(summary.balance)}
      </strong>
    </div>
  );
}

function CategoryBar({ item, maxTotal }) {
  const { formatCurrency } = useCurrency();
  const width = Math.max(6, Math.round((item.total / maxTotal) * 100));

  return (
    <div className="category-bar-row">
      <div className="category-bar-heading">
        <strong>{item.categoryName}</strong>
        <span>{formatCurrency(item.total)}</span>
      </div>
      <div className="category-bar-track">
        <span style={{ width: `${width}%` }} />
      </div>
    </div>
  );
}

function BudgetProgressRow({ budget, formatCurrency }) {
  const progress = Math.min(100, budget.progressPercent);
  const isOverBudget = budget.progressPercent > 100;

  return (
    <div className="budget-row">
      <div className="category-bar-heading">
        <strong>{budget.categoryName}</strong>
        <span>
          {formatCurrency(budget.spentAmount)} / {formatCurrency(budget.budgetAmount)}
        </span>
      </div>
      <div className={`category-bar-track budget-track ${isOverBudget ? 'over' : ''}`}>
        <span style={{ width: `${Math.max(4, progress)}%` }} />
      </div>
      <div className="budget-meta">
        <span>{budget.progressPercent}% utilise</span>
        <strong className={`amount ${budget.remainingAmount >= 0 ? 'income' : 'expense'}`}>
          {formatCurrency(budget.remainingAmount)} restant
        </strong>
      </div>
    </div>
  );
}
