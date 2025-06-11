import './App.css'
import {HashRouter as Router, Routes , Route} from 'react-router-dom'
import {Layout} from './components/Layout'
import {Home} from './pages/Home'
import {Device} from './pages/Device'
import {Login} from './pages/LoginForm'
import { Register } from './pages/Register'
import { AddDevice } from './pages/Add-device'



function App() {
  return (
    <Router>
      <Routes>
        <Route element={<Layout />}>
          <Route path="/" element={<Home />} />
          <Route path="/Login" element={<Login />} />
          <Route path="/Device" element={<Device />} />
          <Route path="/add-device" element={<AddDevice />} />
          <Route path="/Register" element={<Register />} />
        </Route>
      </Routes>
    </Router>
  )
}

export default App
