'use client';
import Image from "next/image";
import { useRouter, useSearchParams } from 'next/navigation'
import { useEffect } from "react";
import ReactDOM from "react-dom"
import { useForm, SubmitHandler } from "react-hook-form"
import axios from '@/lib/axios';


interface IFormInput {
  username: string;
  password: string;
  remember?: boolean;
}

const baseUrl = process.env.NEXT_PUBLIC_API_BASE_URL;


export default function Home() {
  const router = useRouter()
  const searchParams = useSearchParams()
  const { register, handleSubmit } = useForm<IFormInput>()
  const onSubmit: SubmitHandler<IFormInput> = async (data) => {
    // const res = await axios.post(`/login`, {
    //   password: data.password,
    //   remember: data.remember,
    //   username: data.username,
    // });
    const res = await axios.post(`/login`, data);
    
    if (res.data && res.status === 200) {
      const result = await res.data;
      document.cookie = `token=${result.token}; path=/; max-age=${result.expiresIn * 60};`;
      router.push('/home')
    } else {
      alert('Login failed');
    }
  }
  const handleLineLogin = () => {
    const client_id = process.env.NEXT_PUBLIC_LINE_CLIENT_ID;
    const redirect_uri = process.env.NEXT_PUBLIC_LINE_REDIRECT_URI;
    const state = 'random_string_or_csrf_token';
    const scope = 'profile openid email';

    const lineLoginUrl = `https://access.line.me/oauth2/v2.1/authorize?response_type=code&client_id=${client_id}&redirect_uri=${redirect_uri}&state=${state}&scope=${scope}`

    window.location.href = lineLoginUrl
  }
  useEffect(() => {
    const token = searchParams.get('token')
    const expiresIn = Number(searchParams.get('expiresIn')) || 60;
    if (token) {
      document.cookie = `token=${token}; path=/; max-age=${expiresIn * 60};`;
      router.push('/home')
    }
  }, [searchParams])
  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-blue-100 via-white to-blue-200 dark:from-gray-900 dark:via-gray-800 dark:to-gray-900">
      <form onSubmit={handleSubmit(onSubmit)} className="w-full max-w-sm bg-white dark:bg-gray-800 rounded-xl shadow-lg p-8 space-y-6">
        <h2 className="text-2xl font-bold text-center text-blue-700 dark:text-white mb-6">Sign In</h2>
        <div>
          <input
            { ...register("username")} 
            type="text"
            className="bg-gray-50 border border-gray-300 text-gray-900 text-sm rounded-lg focus:ring-blue-500 focus:border-blue-500 block w-full p-2.5 dark:bg-gray-700 dark:border-gray-600 dark:placeholder-gray-400 dark:text-white"
            required
            placeholder="Username"
          />
        </div>
        <div>
          <input
            { ...register("password")}
            type="password"
            className="bg-gray-50 border border-gray-300 text-gray-900 text-sm rounded-lg focus:ring-blue-500 focus:border-blue-500 block w-full p-2.5 dark:bg-gray-700 dark:border-gray-600 dark:placeholder-gray-400 dark:text-white"
            required
            placeholder="Password"
          />
        </div>
        <div className="flex items-center">
          <input
            { ...register("remember") }
            id="remember"
            type="checkbox"
            className="w-4 h-4 border border-gray-300 rounded-sm bg-gray-50 focus:ring-3 focus:ring-blue-300 dark:bg-gray-700 dark:border-gray-600 dark:focus:ring-blue-600"
          />
          <label htmlFor="remember" className="ms-2 text-sm font-medium text-gray-900 dark:text-gray-300">
            Remember me
          </label>
        </div>
        <button
          type="submit"
          className="w-full text-white bg-blue-700 hover:bg-blue-800 focus:ring-4 focus:outline-none focus:ring-blue-300 font-medium rounded-lg text-sm px-5 py-2.5 text-center dark:bg-blue-600 dark:hover:bg-blue-700 dark:focus:ring-blue-800 transition"
        >
          Submit
        </button>
        <button
        onClick={handleLineLogin}
          type="button"
          className="w-full text-white bg-green-700 hover:bg-green-800 focus:ring-4 focus:outline-none focus:ring-green-300 font-medium rounded-lg text-sm px-5 py-2.5 text-center dark:bg-green-600 dark:hover:bg-green-700 dark:focus:ring-green-800 transition"
        >
          Line
        </button>
      </form>
    </div>
  );
}
