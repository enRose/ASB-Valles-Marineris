import React from 'react';
import { connect } from 'react-redux';

const Home = props => (
  <div>
    <h1>Hello, I'm Valles Marineris</h1>
    <p>Welcome to Mars:</p>

        <div class="input-group">
            <span
                class="input-group-addon"
                id="basic-addon1">@You
            </span>
            <input
                type="text"
                class="form-control"
                placeholder="Say something"
                aria-describedby="basic-addon1">
            </input>
        </div>

        <div class="input-group">
            <input
                type="text"
                class="form-control"
                placeholder="Bot says..."
                aria-describedby="basic-addon2">
            </input>

            <span
                class="input-group-addon"
                id="basic-addon2">
                @VM
            </span>
        </div>
  </div>
);

export default connect()(Home);
