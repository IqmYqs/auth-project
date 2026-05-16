import { NextResponse } from 'next/server'
import type { NextRequest } from 'next/server'

const baseUrl = process.env.NEXT_PUBLIC_API_BASE_URL;

export async function middleware(request: NextRequest) {
  const token = request.cookies.get('token')?.value

  if(!token && request.nextUrl.pathname != '/') {
    // ถ้าไม่มี token ให้ redirect ไปหน้า login
    return NextResponse.redirect(new URL('/', request.url))
  }else if (token) {
    const res = await fetch(`${baseUrl}/CheckToken?token=${token}`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },  
    });
    if (!res.ok) {
        const response = NextResponse.redirect(new URL('/', request.url))
        response.cookies.set('token', '', { maxAge: 0, path: '/' })
        return response
    }else if (request.nextUrl.pathname == '/') {
        return NextResponse.redirect(new URL('/home', request.url))
    }
  }
  return NextResponse.next()
}

// กำหนด path ที่จะให้ middleware ทำงาน
export const config = {
//   matcher: ['/home/:path*'],
  matcher: [
    '/((?!api|_next/static|_next/image|favicon.ico|register).*)',
    // '/((?!api|_next/static|_next/image|favicon.ico|login|register).*)',
  ],
}