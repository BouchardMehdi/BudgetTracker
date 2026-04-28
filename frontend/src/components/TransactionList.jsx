import { Pencil, Trash2 } from 'lucide-react';
import { Link } from 'react-router-dom';
import { useCurrency } from '../context/CurrencyContext';

const dateFormatter = new Intl.DateTimeFormat('fr-FR');

export default function TransactionList({ transactions, onDelete }) {
  const { formatCurrency } = useCurrency();

  if (transactions.length === 0) {
    return <div className="empty-state">Aucune transaction.</div>;
  }

  return (
    <div className="table-wrap">
      <table className="transactions-table">
        <thead>
          <tr>
            <th>Titre</th>
            <th>Categorie</th>
            <th>Date</th>
            <th>Type</th>
            <th>Montant</th>
            <th aria-label="Actions" />
          </tr>
        </thead>
        <tbody>
          {transactions.map((transaction) => (
            <tr key={transaction.id}>
              <td>
                <strong>{transaction.title}</strong>
                {transaction.description && <span>{transaction.description}</span>}
                {(transaction.isRecurring || transaction.recurringParentId) && (
                  <span className="mini-badge">
                    {transaction.recurringParentId ? 'Auto mensuel' : 'Mensuel'}
                  </span>
                )}
              </td>
              <td>{transaction.categoryName}</td>
              <td>{dateFormatter.format(new Date(transaction.transactionDate))}</td>
              <td>
                <span className={`pill ${transaction.type}`}>
                  {transaction.type === 'income' ? 'Revenu' : 'Depense'}
                </span>
              </td>
              <td className={`amount ${transaction.type}`}>
                {formatCurrency(transaction.amount)}
              </td>
              <td>
                <div className="row-actions">
                  <Link
                    className="icon-button"
                    to={`/transactions/${transaction.id}/edit`}
                    title="Modifier"
                    aria-label={`Modifier ${transaction.title}`}
                  >
                    <Pencil size={18} aria-hidden="true" />
                  </Link>
                  <button
                    className="icon-button danger"
                    type="button"
                    onClick={() => onDelete(transaction)}
                    title="Supprimer"
                    aria-label={`Supprimer ${transaction.title}`}
                  >
                    <Trash2 size={18} aria-hidden="true" />
                  </button>
                </div>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
