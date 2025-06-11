import { Link } from "react-router-dom"
import "./Navbar.css"

export function Navbar(){
    return(
        <div className="navbar">
        <Link to="/"><button className="navbutton">Головна</button></Link>
        <Link to="/Device"><button className="navbutton">Контроль пристрою</button></Link>
        <Link to="/Login"><button className="navbutton">Логін</button></Link>
        <Link to="/Register"><button className="navbutton">Реестрація</button></Link>
        </div>
    )
}

