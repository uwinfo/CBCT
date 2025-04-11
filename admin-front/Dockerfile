FROM node:18

WORKDIR /app
ADD . /app/

RUN npm i -g npm@6.13.7 && npm i && npm rebuild node-sass
ARG NODE_ENV
RUN NODE_ENV=${NODE_ENV} npm run build

EXPOSE 5100
CMD ["npm", "start"]