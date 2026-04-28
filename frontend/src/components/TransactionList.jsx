import { Trash2 } from 'lucide-react';

const currencyFormatter = new Intl.NumberFormat('fr-FR', {
  style: 'currency',
  currency: 'EUR',
});

const dateFormatter = new Intl.DateTimeFormat('fr-FR');

export default function TransactionList({ transactions, onDelete }) {
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
              </td>
              <td>{transaction.categoryName}</td>
              <td>{dateFormatter.format(new Date(transaction.transactionDate))}</td>
              <td>
                <span className={`pill ${transaction.type}`}>
                  {transaction.type === 'income' ? 'Revenu' : 'Depense'}
                </span>
              </td>
              <td className={`amount ${transaction.type}`}>
                {currencyFormatter.format(transaction.amount)}
              </td>
              <td>
                <button
                  className="icon-button danger"
                  type="button"
                  onClick={() => onDelete(transaction.id)}
                  title="Supprimer"
                  aria-label={`Supprimer ${transaction.title}`}
                >
                  <Trash2 size={18} aria-hidden="true" />
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
