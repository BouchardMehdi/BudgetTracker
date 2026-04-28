import { Save } from 'lucide-react';
import { useEffect, useMemo, useState } from 'react';
import { getApiErrorMessage } from '../utils/apiError';

const initialForm = {
  title: '',
  amount: '',
  type: 'expense',
  transactionDate: new Date().toISOString().slice(0, 10),
  description: '',
  categoryId: '',
  isRecurring: false,
  recurrenceStartDate: '',
  recurrenceEndDate: '',
};

function formatInitialValues(values) {
  if (!values) {
    return initialForm;
  }

  return {
    title: values.title || '',
    amount: values.amount?.toString() || '',
    type: values.type || 'expense',
    transactionDate: values.transactionDate?.slice(0, 10) || initialForm.transactionDate,
    description: values.description || '',
    categoryId: values.categoryId?.toString() || '',
    isRecurring: Boolean(values.isRecurring),
    recurrenceStartDate: values.recurrenceStartDate?.slice(0, 10) || values.transactionDate?.slice(0, 10) || '',
    recurrenceEndDate: values.recurrenceEndDate?.slice(0, 10) || '',
  };
}

export default function TransactionForm({
  categories,
  onSubmit,
  initialValues = null,
  isSubmitting = false,
  submitLabel = 'Enregistrer',
  resetOnSuccess = true,
}) {
  const [form, setForm] = useState(() => formatInitialValues(initialValues));
  const [formError, setFormError] = useState('');
  const [fieldErrors, setFieldErrors] = useState({});

  useEffect(() => {
    setForm(formatInitialValues(initialValues));
    setFieldErrors({});
    setFormError('');
  }, [initialValues]);

  const availableCategories = useMemo(
    () => categories.filter((category) => category.type === form.type),
    [categories, form.type]
  );

  function updateField(event) {
    const { name, value, type, checked } = event.target;
    setFieldErrors((current) => ({ ...current, [name]: '' }));
    setForm((current) => ({
      ...current,
      [name]: type === 'checkbox' ? checked : value,
      ...(name === 'transactionDate' && current.isRecurring && !current.recurrenceStartDate
        ? { recurrenceStartDate: value }
        : {}),
      ...(name === 'isRecurring' && checked && !current.recurrenceStartDate
        ? { recurrenceStartDate: current.transactionDate }
        : {}),
      ...(name === 'type' ? { categoryId: '' } : {}),
    }));
  }

  async function handleSubmit(event) {
    event.preventDefault();
    setFormError('');

    const validationErrors = {};

    if (!form.title.trim()) {
      validationErrors.title = 'Le titre est obligatoire.';
    }

    if (Number(form.amount) <= 0) {
      validationErrors.amount = 'Le montant doit etre superieur a 0.';
    }

    if (!form.transactionDate) {
      validationErrors.transactionDate = 'La date est obligatoire.';
    }

    if (!form.categoryId) {
      validationErrors.categoryId = 'Choisis une categorie.';
    }

    if (form.isRecurring) {
      const startDate = form.recurrenceStartDate || form.transactionDate;
      if (form.recurrenceEndDate && form.recurrenceEndDate < startDate) {
        validationErrors.recurrenceEndDate = 'La date de fin doit etre apres la date de debut.';
      }
    }

    setFieldErrors(validationErrors);

    if (Object.keys(validationErrors).length > 0) {
      return;
    }

    try {
      await onSubmit({
        title: form.title.trim(),
        amount: Number(form.amount),
        type: form.type,
        transactionDate: form.transactionDate,
        description: form.description.trim(),
        categoryId: Number(form.categoryId),
        isRecurring: form.isRecurring,
        recurrenceStartDate: form.isRecurring ? form.recurrenceStartDate || form.transactionDate : null,
        recurrenceEndDate: form.isRecurring && form.recurrenceEndDate ? form.recurrenceEndDate : null,
      });

      if (resetOnSuccess) {
        setForm(initialForm);
      }
    } catch (error) {
      setFormError(getApiErrorMessage(error));
    }
  }

  return (
    <form className="form-panel" onSubmit={handleSubmit}>
      {formError && <div className="alert error">{formError}</div>}

      <div className="form-grid">
        <label>
          Titre
          <input name="title" value={form.title} onChange={updateField} placeholder="Salaire, courses..." />
          {fieldErrors.title && <span className="field-error">{fieldErrors.title}</span>}
        </label>

        <label>
          Montant
          <input name="amount" type="number" min="0.01" step="0.01" value={form.amount} onChange={updateField} />
          {fieldErrors.amount && <span className="field-error">{fieldErrors.amount}</span>}
        </label>

        <label>
          Type
          <select name="type" value={form.type} onChange={updateField}>
            <option value="expense">Depense</option>
            <option value="income">Revenu</option>
          </select>
        </label>

        <label>
          Date
          <input name="transactionDate" type="date" value={form.transactionDate} onChange={updateField} />
          {fieldErrors.transactionDate && <span className="field-error">{fieldErrors.transactionDate}</span>}
        </label>

        <label>
          Categorie
          <select name="categoryId" value={form.categoryId} onChange={updateField}>
            <option value="">Selectionner</option>
            {availableCategories.map((category) => (
              <option key={category.id} value={category.id}>
                {category.name}
              </option>
            ))}
          </select>
          {fieldErrors.categoryId && <span className="field-error">{fieldErrors.categoryId}</span>}
        </label>

        <label className="full-width">
          Description
          <textarea name="description" value={form.description} onChange={updateField} rows="4" />
        </label>

        <label className="checkbox-label full-width">
          <input
            name="isRecurring"
            type="checkbox"
            checked={form.isRecurring}
            onChange={updateField}
          />
          Repeter chaque mois
        </label>

        {form.isRecurring && (
          <>
            <label>
              Date de debut
              <input
                name="recurrenceStartDate"
                type="date"
                value={form.recurrenceStartDate || form.transactionDate}
                onChange={updateField}
              />
            </label>

            <label>
              Date de fin
              <input
                name="recurrenceEndDate"
                type="date"
                value={form.recurrenceEndDate}
                onChange={updateField}
              />
              {fieldErrors.recurrenceEndDate && <span className="field-error">{fieldErrors.recurrenceEndDate}</span>}
            </label>
          </>
        )}
      </div>

      <button className="primary-button" type="submit" disabled={isSubmitting}>
        <Save size={18} aria-hidden="true" />
        {isSubmitting ? 'Enregistrement...' : submitLabel}
      </button>
    </form>
  );
}
