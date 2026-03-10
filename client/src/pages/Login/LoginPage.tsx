import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { login } from '../../api/auth';
import { useTranslation } from 'react-i18next';
import { useAuth } from '../../context/AuthContext';

const schema = z.object({
  email: z.string().email('Invalid email'),
  password: z.string().min(1, 'Password is required'),
});

type FormData = z.infer<typeof schema>;

export function LoginPage() {
  const { t } = useTranslation();
  const [error, setError] = useState('');
  const navigate = useNavigate();
  const { login: authLogin } = useAuth();

  const { register, handleSubmit, formState: { errors } } = useForm<FormData>({
    resolver: zodResolver(schema),
  });

  const onSubmit = async (data: FormData) => {
    setError('');
    try {
      const res = await login(data.email, data.password);
      authLogin(res.token, res.email, res.role);
      navigate('/');
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Login failed');
    }
  };

  return (
    <div className="py-6 max-w-md mx-auto">
      <h1 className="text-2xl font-bold text-amber-50 mb-4">{t('auth.login')}</h1>
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
            {...register('password')}
            className="w-full px-4 py-2 rounded-xl bg-amber-900/50 border border-amber-700/50 text-amber-50 outline-none focus:ring-2 focus:ring-amber-500"
          />
          {errors.password && <p className="text-red-400 text-sm mt-1">{errors.password.message}</p>}
        </div>
        <button
          type="submit"
          className="w-full py-2 rounded-xl bg-amber-600 hover:bg-amber-500 text-amber-950 font-semibold"
        >
          Log in
        </button>
      </form>
      <p className="mt-4 text-amber-200 text-center">
        {t('auth.noAccount')}{' '}
        <Link to="/register" className="underline text-amber-300 hover:text-amber-200">
          {t('auth.register')}
        </Link>
      </p>
    </div>
  );
}
