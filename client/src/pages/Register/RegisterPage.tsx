import { useMemo, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { register as apiRegister } from '../../api/auth';
import { useTranslation } from 'react-i18next';
import { useAuth } from '../../context/AuthContext';

const schema = z.object({
  email: z.string().email('Invalid email'),
  password: z
    .string()
    .min(6, 'At least 6 characters')
    .regex(/[A-Z]/, 'At least one uppercase letter')
    .regex(/[a-z]/, 'At least one lowercase letter'),
});

type FormData = z.infer<typeof schema>;

type PasswordRuleKey = 'minLength' | 'uppercase' | 'lowercase';

/** Matches only the three rules on the password schema above. */
function evaluatePasswordRules(password: string): Record<PasswordRuleKey, boolean> {
  return {
    minLength: password.length >= 6,
    uppercase: /[A-Z]/.test(password),
    lowercase: /[a-z]/.test(password),
  };
}

export function RegisterPage() {
  const { t } = useTranslation();
  const [error, setError] = useState('');
  const navigate = useNavigate();
  const { login } = useAuth();

  const { register, handleSubmit, watch, formState: { errors } } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: { email: '', password: '' },
  });

  const passwordValue = watch('password') ?? '';

  const passwordChecks = useMemo(
    () => evaluatePasswordRules(passwordValue),
    [passwordValue],
  );

  const ruleItems: { key: PasswordRuleKey }[] = [
    { key: 'minLength' },
    { key: 'uppercase' },
    { key: 'lowercase' },
  ];

  const onSubmit = async (data: FormData) => {
    setError('');
    try {
      const res = await apiRegister(data.email, data.password);
      login(res.token, res.email, res.role);
      navigate('/');
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Registration failed');
    }
  };

  return (
    <div className="py-6 max-w-md mx-auto">
      <h1 className="text-2xl font-bold text-amber-50 mb-4">{t('auth.register')}</h1>
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        {error && (
          <div className="p-3 rounded-lg bg-red-900/50 text-red-200 text-sm">{error}</div>
        )}
        <div>
          <label className="block text-amber-200 text-sm mb-1">{t('auth.email')}</label>
          <input
            type="email"
            {...register('email')}
            className="w-full px-4 py-2 rounded-xl bg-amber-900/50 border border-amber-700/50 text-amber-50 outline-none focus:ring-2 focus:ring-amber-500"
          />
          {errors.email && <p className="text-red-400 text-sm mt-1">{errors.email.message}</p>}
        </div>
        <div>
          <label className="block text-amber-200 text-sm mb-1">{t('auth.password')}</label>
          <input
            type="password"
            autoComplete="new-password"
            {...register('password')}
            className="w-full px-4 py-2 rounded-xl bg-amber-900/50 border border-amber-700/50 text-amber-50 outline-none focus:ring-2 focus:ring-amber-500"
          />
          <ul className="mt-2 space-y-1.5 text-sm" aria-live="polite">
            {ruleItems.map(({ key }) => {
              const ok = passwordChecks[key];
              return (
                <li
                  key={key}
                  className={`flex items-center gap-2 ${ok ? 'text-emerald-400' : 'text-red-400'}`}
                >
                  <span className="shrink-0 w-4 text-center" aria-hidden>
                    {ok ? '✓' : '✕'}
                  </span>
                  <span>{t(`auth.passwordRules.${key}`)}</span>
                </li>
              );
            })}
          </ul>
          {errors.password && (
            <p className="text-red-400 text-sm mt-2">{errors.password.message}</p>
          )}
        </div>
        <button
          type="submit"
          className="w-full py-2 rounded-xl bg-amber-600 hover:bg-amber-500 text-amber-950 font-semibold"
        >
          Register
        </button>
      </form>
      <p className="mt-4 text-amber-200 text-center">
        {t('auth.haveAccount')}{' '}
        <Link to="/login" className="underline text-amber-300 hover:text-amber-200">
          {t('auth.login')}
        </Link>
      </p>
    </div>
  );
}
